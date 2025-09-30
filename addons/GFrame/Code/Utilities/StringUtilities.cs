public static class StringUtilities
{
    public static char IntegerToLetter(int value)
    {
        if (value < 0 || value > 26)
            return '\0';

        return (char)(value + 'A');
    }

    public static int LetterToInteger(char value)
    {
        if (value >= 'a' && value <= 'z')
            return value - 'a';

        if (value >= 'A' && value <= 'Z')
            return value - 'A';

        return -1;
    }
}
