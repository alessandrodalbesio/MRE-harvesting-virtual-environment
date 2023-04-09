using UnityEngine;
using System.IO;

namespace VirtualEnvironment {
	public class CharWordReader {
		public char[] word;
		public int wordSize;
		public bool endReached;

		private StreamReader reader; /* File reference */
		private int bufferSize; /* Size of each chunk of read data from the file */
		private char[] buffer; /* Chunk of data read from the file */
		
		public char currentChar; /* Current character */
		private int currentPosition = 0; /* Position in the buffer */
		private int maxPosition = 0; /* Number of read characters from the buffer (currentPosition is <= maxPosition) */

		public CharWordReader(StreamReader reader, int bufferSize) {
			this.reader = reader;
			this.bufferSize = bufferSize;

			this.buffer = new char[this.bufferSize];
			this.word = new char[this.bufferSize];

			this.MoveNext();
		}

		public void SkipWhitespaces() {
            /* Skip all the white spaces */
			while (char.IsWhiteSpace(this.currentChar)) {
				this.MoveNext();
			}
		}

		public void SkipWhitespaces(out bool newLinePassed) {
			newLinePassed = false;
			while (char.IsWhiteSpace(this.currentChar)) {
				if (this.currentChar == '\r' || this.currentChar == '\n') {
					newLinePassed = true;
				}
				this.MoveNext();
			}
		}

		public void SkipUntilNewLine() {
			while (this.currentChar != char.MinValue && this.currentChar != '\n' && this.currentChar != '\r') {
				this.MoveNext();
			}
			this.SkipNewLineSymbols();
		}

        /* Read untill we find a white space */
		public void ReadUntilWhiteSpace() {
			this.wordSize = 0;
			while (this.currentChar != char.MinValue && char.IsWhiteSpace(this.currentChar) == false) {
				this.word[this.wordSize] = this.currentChar;
				this.wordSize++;
				this.MoveNext();
			}
		}

        /* Read untill we find a new line symbol */
		public void ReadUntilNewLine() {
			this.wordSize = 0;
			while (this.currentChar != char.MinValue && this.currentChar != '\n' && this.currentChar != '\r') {
				this.word[this.wordSize] = this.currentChar;
				this.wordSize++;
				this.MoveNext();
			}
			this.SkipNewLineSymbols();
		}

        /* Verify if the string that has been read is the same as the one passed as parameter */
		public bool Is(string other) {
            /* If the length is different, the strings are different */
			if (other.Length != this.wordSize) {
				return false;
			}

            /* Compare each character */
			for (int i=0; i<this.wordSize; i++) {
				if (this.word[i] != other[i]) {
					return false;
				}
			}

			return true;
		}


        /* Return a string of length wordSize - startIndex */
        public string GetString(int startIndex = 0) {
            if (startIndex >= this.wordSize - 1) {
                return string.Empty;
            }
            return new string(this.word, startIndex, this.wordSize - startIndex);
        }
		
        /* Read a vector */
		public Vector3 ReadVector() {
			this.SkipWhitespaces();
			float x = this.ReadFloat();
			this.SkipWhitespaces();
			float y = this.ReadFloat();
			this.SkipWhitespaces(out var newLinePassed);
			float z = 0f;
			if (newLinePassed == false) {
				z = this.ReadFloat();
			}
			return new Vector3(x, y, z);
		}

        /* Read an integer number */
		public int ReadInt() {
			int result = 0;
			bool isNegative = this.currentChar == '-';
			if (isNegative == true) {
				this.MoveNext();
			}
			
			while (this.currentChar >= '0' && this.currentChar <= '9') {
				var digit = this.currentChar - '0';
				result = result * 10 + digit;
				this.MoveNext();
			}

			return (isNegative == true) ? -result : result;
		}

        /* Read a float number */
		public float ReadFloat() {
			bool isNegative = this.currentChar == '-';
			if (isNegative) {
				this.MoveNext();
			}

			var num = (float)this.ReadInt(); /* Read the number before the dot or the comma */
			if (this.currentChar == '.' || this.currentChar == ',') {
				this.MoveNext();
				num +=  this.ReadFloatEnd();
                
                /* Read the number after the exponent */
				if (this.currentChar == 'e' || this.currentChar == 'E') {
					this.MoveNext();
					var exp = this.ReadInt();
					num = num * Mathf.Pow(10f, exp);
				}
			}
			if (isNegative == true) {
				num = -num;
			}

			return num;
		}

        /* Read a float number after the comma or the dot */
		private float ReadFloatEnd() {
			float result = 0f;

			var exp = 0.1f;
			while (this.currentChar >= '0' && this.currentChar <= '9') {
				var digit = this.currentChar - '0';
				result += digit * exp;

				exp *= 0.1f;

				this.MoveNext();
			}

			return result;
		}

        /* Skip all the new line symbols */
		private void SkipNewLineSymbols() {
			while (this.currentChar == '\n' || this.currentChar == '\r') {
				this.MoveNext();
			}
		}

		public void MoveNext() {
            /* Increment the position we are in the buffer */
			this.currentPosition++;

            /* If the current position is more than the number of read characters, we read a new buffer */
			if (this.currentPosition >= this.maxPosition) {
				if (this.reader.EndOfStream == true) {
                    /* We have reached the end of the file */
					this.currentChar = char.MinValue;
					this.endReached = true;
					return;
				}

                /* Reset the position and read a new buffer */
				this.currentPosition = 0;
				this.maxPosition = this.reader.Read(this.buffer, 0, this.bufferSize); /* Maximum position is the number of read characters (it can be less then bufferSize) */
			}
			this.currentChar = this.buffer[this.currentPosition]; /* Update the character with a new one */
		}
	}
}