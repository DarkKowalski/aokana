using System;

namespace Aokana
{
    public enum GameDlcVersion
    {
        NoDlc,
        Extra1,
        Extra2
    }

    public interface IExtractor
    {
        int ExtractAll(string dataFilePath, string toDirectory);
    }

    public class ExtractorFactory
    {
        public static IExtractor GetExtractor(GameDlcVersion version)
        {
            switch (version)
            {
                case GameDlcVersion.NoDlc:
                case GameDlcVersion.Extra1:
                    return new Extractor();
                case GameDlcVersion.Extra2:
                    return new ExtractorExtra2();
                default:
                    throw new ArgumentException("Invalid version");
            }
        }
    }
}

