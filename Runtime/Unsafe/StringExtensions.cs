using UnityEngine.Assertions;

namespace UnityExtensions.Unsafe
{
    public static class StringUtils
    {
        //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Convert.cs
        #region dotnet
        /// <summary>
        /// Compute the number of bytes encoded in the specified Base 64 char array:
        /// Walk the entire input counting white spaces and padding chars, then compute result length
        /// based on 3 bytes per 4 chars.
        /// </summary>
        public static unsafe int FromBase64_ComputeResultLength(char* inputPtr, int inputLength)
        {
            const uint intEq = '=';
            const uint intSpace = ' ';

            Assert.IsTrue(0 <= inputLength);

            char* inputEndPtr = inputPtr + inputLength;
            int usefulInputLength = inputLength;
            int padding = 0;

            while (inputPtr < inputEndPtr)
            {
                uint c = *inputPtr;
                inputPtr++;

                // We want to be as fast as possible and filter out spaces with as few comparisons as possible.
                // We end up accepting a number of illegal chars as legal white-space chars.
                // This is ok: as soon as we hit them during actual decode we will recognise them as illegal and throw.
                if (c <= intSpace)
                    usefulInputLength--;
                else if (c == intEq)
                {
                    usefulInputLength--;
                    padding++;
                }
            }

            Assert.IsTrue(0 <= usefulInputLength);

            // For legal input, we can assume that 0 <= padding < 3. But it may be more for illegal input.
            // We will notice it at decode when we see a '=' at the wrong place.
            Assert.IsTrue(0 <= padding);

            // Perf: reuse the variable that stored the number of '=' to store the number of bytes encoded by the
            // last group that contains the '=':
            if (padding != 0)
            {
                if (padding == 1)
                    padding = 2;
                else if (padding == 2)
                    padding = 1;
                //else
                    //throw new FormatException(SR.Format_BadBase64Char);
            }

            // Done:
            return (usefulInputLength / 4) * 3 + padding;
        }
        #endregion // dotnet
    }

    public static class StringExtensions
    {
        public static unsafe int FromBase64_ComputeResultLength(this string input)
        {
            fixed (char* p = input)
            {
                return StringUtils.FromBase64_ComputeResultLength(p, input.Length);
            }
        }
    }
}
