using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace Kavod.Vba.Compression
{
    internal static class Tokenizer
    {
        internal static IEnumerable<TokenSequence> ToTokenSequences(this IEnumerable<IToken> tokens)
        {
            var accumulatedTokens = new List<IToken>();
            foreach (var t in tokens)
            {
                if (accumulatedTokens.Count == 8)
                {
                    yield return new TokenSequence(accumulatedTokens);
                    accumulatedTokens.Clear();
                }
                accumulatedTokens.Add(t);
            }
            if (accumulatedTokens.Count != 0)
            {
                yield return new TokenSequence(accumulatedTokens);
            }
        }

        internal static void DecompressTokenSequence(this IEnumerable<IToken> tokens, BinaryWriter writer)
        {
            foreach (var token in tokens)
            {
                token.DecompressToken(writer);
            }
        }

        internal static bool OverlapsWith(this CopyToken thisToken, CopyToken otherToken)
        {
            var firstToken = thisToken;
            var secondToken = otherToken;
            if (thisToken.Position > otherToken.Position)
            {
                firstToken = otherToken;
                secondToken = thisToken;
            }
            Contract.Assert(firstToken.Position <= secondToken.Position);

            return firstToken.Position + firstToken.Length > secondToken.Position;
        }

        internal static bool Contains(this CopyToken thisToken, CopyToken otherToken)
        {
            var otherTokenStartsAfterThisToken = thisToken.Position <= otherToken.Position;
            var otherTokenEndsBeforeThisToken = thisToken.Position + thisToken.Length >=
                                                otherToken.Position + otherToken.Length;
            return otherTokenStartsAfterThisToken && otherTokenEndsBeforeThisToken;
        }

        internal static IEnumerable<IToken> TokenizeUncompressedData(byte[] uncompressedData)
        {
            // The commented code is alternative to the specification for the compression.
            //var possibleCopyTokens = AllPossibleCopyTokens(uncompressedData);
            //var normalCopyTokens = NormalizeCopyTokens(possibleCopyTokens);
            //var allTokens = WeaveTokens(normalCopyTokens, uncompressedData);

            var copyTokens = GetSpecificationCopyTokens(uncompressedData);
            var allTokens = WeaveTokens(copyTokens, uncompressedData);
            foreach (var t in allTokens)
            {
                yield return t;
            }
        }

        private static IEnumerable<CopyToken> GetSpecificationCopyTokens(byte[] uncompressedData)
        {
            var position = 0L;
            while (position < uncompressedData.Length)
            {
                UInt16 offset = 0;
                UInt16 length = 0;
                Match(uncompressedData, position, out offset, out length);

                if (length > 0)
                {
                    yield return new CopyToken(position, offset, length);
                    position += length;
                }
                else
                {
                    position++;
                }
            }
        }

        private static IEnumerable<CopyToken> AllPossibleCopyTokens(byte[] uncompressedData)
        {
            var position = 0L;
            while (position < uncompressedData.Length)
            {
                UInt16 offset = 0;
                UInt16 length = 0;
                Match(uncompressedData, position, out offset, out length);
                
                if (length > 0)
                {
                    yield return new CopyToken(position, offset, length);
                }
                position++;
            }
        }

        private static IEnumerable<CopyToken> NormalizeCopyTokens(IEnumerable<CopyToken> copyTokens)
        {
            var remainingTokens = RemoveRedundantTokens(copyTokens).ToList();

            remainingTokens = RemoveOverlappingTokens(remainingTokens).ToList();

            return remainingTokens;
        }

        private static IEnumerable<CopyToken> RemoveRedundantTokens(IEnumerable<CopyToken> tokens)
        {
            CopyToken previous = null;
            foreach (var next in tokens)
            {
                if (previous == null)
                {
                    previous = next;
                    continue;
                }
                if (previous.OverlapsWith(next))
                {
                    //figure out which one to keep.  There can only be one!
                    if (previous.Length >= next.Length)
                    {
                        yield return previous;
                        // can't return next.
                    }
                    else
                    {
                        yield return next;
                    }
                }
                else
                {
                    yield return previous;
                    previous = next;
                }
            }
        }

        private static IEnumerable<CopyToken> RemoveOverlappingTokens(IEnumerable<CopyToken> tokens)
        {
            // create a list of the current tokens.
            Node list = null;
            foreach (var t in tokens.Reverse())
            {
                list = new Node(t, list);
            }
            Contract.Assert(list != null);

            return FindBestPath(list);
        }

        private static Node FindBestPath(Node node)
        {
            Contract.Requires<ArgumentNullException>(node != null);

            // find any overlapping tokens
            Node bestPath = null;
            foreach (var overlappingNode in GetOverlappingNodes(node))
            {
                var currentPath = new Node(overlappingNode.Value, null);

                // find the next non-overlapping node.
                var nonOverlappingNode = GetNextNonOverlappingNode(overlappingNode);
                if (nonOverlappingNode != null)
                {
                    currentPath.NextNode = FindBestPath(nonOverlappingNode);
                }

                if (bestPath == null 
                    || bestPath.Length < currentPath.Length)
                {
                    bestPath = currentPath;
                }
            }
            return bestPath;
        }

        private static IEnumerable<Node> GetOverlappingNodes(Node node)
        {
            Contract.Requires<ArgumentNullException>(node != null);

            var firstNode = node;

            while (node != null 
                && firstNode.Value.OverlapsWith(node.Value))
            {
                yield return node;
                node = node.NextNode;
            }
        }

        private static Node GetNextNonOverlappingNode(Node node)
        {
            Contract.Requires<ArgumentNullException>(node != null);

            var firstNode = node;

            while (node != null 
                && firstNode.Value.OverlapsWith(node.Value))
            {
                node = node.NextNode;
            }
            return node;
        }

        private static IEnumerable<IToken> WeaveTokens(IEnumerable<CopyToken> copyTokens, byte[] uncompressedData)
        {
            var position = 0L;
            foreach (var currentCopyToken in copyTokens)
            {
                while (position < currentCopyToken.Position)
                {
                    yield return new LiteralToken(uncompressedData[position]);
                    position++;
                }
                yield return currentCopyToken;
                position += currentCopyToken.Length;
            }
            while (position < uncompressedData.Length)
            {
                yield return new LiteralToken(uncompressedData[position]);
                position++;
            }
        }

        internal static void Match(byte[] uncompressedData, long position, out UInt16 matchedOffset, out UInt16 matchedLength)
        {
            var decompressedCurrent = position;
            var decompressedEnd = uncompressedData.Length;
            const long decompressedChunkStart = 0;

            // SET Candidate TO DecompressedCurrent MINUS 1
            var candidate = decompressedCurrent - 1L;
            // SET BestLength TO 0
            var bestLength = 0L;
            var bestCandidate = 0L;

            // WHILE Candidate is GREATER THAN OR EQUAL TO DecompressedChunkStart
            while (candidate >= decompressedChunkStart)
            {
                // SET C TO Candidate
                var c = candidate;
                // SET D TO DecompressedCurrent
                var d = decompressedCurrent;
                // SET Len TO 0
                var len = 0;

                // WHILE (D is LESS THAN DecompressedEnd)
                // and (the byte at D EQUALS the byte at C)
                while (d < decompressedEnd
                       && uncompressedData[d] == uncompressedData[c])
                {
                    // INCREMENT Len
                    len++;
                    // INCREMENT C
                    c++;
                    // INCREMENT D
                    d++;
                } // END WHILE

                // IF Len is GREATER THAN BestLength THEN
                if (len > bestLength)
                {
                    // SET BestLength TO Len
                    bestLength = len;
                    // SET BestCandidate TO Candidate
                    bestCandidate = candidate;
                } // ENDIF

                // DECREMENT Candidate
                candidate--;
            } // END WHILE

            // IF BestLength is GREATER THAN OR EQUAL TO 3 THEN
            if (bestLength >= 3)
            {
                // CALL CopyToken Help (section 2.4.1.3.19.1) returning MaximumLength
                var result = CopyToken.CopyTokenHelp(decompressedCurrent);

                // SET Length TO the MINIMUM of BestLength and MaximumLength
                matchedLength = (UInt16)bestLength;
                if (bestLength > result.MaximumLength)
                    matchedLength = result.MaximumLength;

                // SET Offset TO DecompressedCurrent MINUS BestCandidate
                matchedOffset = (UInt16)(decompressedCurrent - bestCandidate);
            }
            else // ELSE
            {
                // SET Length TO 0
                matchedLength = 0;
                // SET Offset TO 0
                matchedOffset = 0;
            } // ENDIF
        }

        #region Private Classes

        private class Node : IEnumerable<CopyToken>
        {
            public Node(CopyToken value, Node nextNode)
            {
                Contract.Requires<ArgumentNullException>(value != null);

                Value = value;
                NextNode = nextNode;
            }

            internal CopyToken Value { get; }

            internal Node NextNode { get; set; }

            internal long Length
            {
                get
                {
                    if (NextNode != null)
                    {
                        return Value.Length + NextNode.Length;
                    }
                    return Value.Length;
                }
            }

            public IEnumerator<CopyToken> GetEnumerator()
            {
                return new NodeEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class NodeEnumerator : IEnumerator<CopyToken>
        {
            private Node _currentNode;
            private Node _nextNode;

            public NodeEnumerator(Node node)
            {
                _nextNode = node;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_nextNode == null)
                {
                    return false;
                }
                _currentNode = _nextNode;
                _nextNode = _nextNode.NextNode;
                return true;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public CopyToken Current => _currentNode.Value;

            object IEnumerator.Current => Current;
        }

        #endregion
    }
}
