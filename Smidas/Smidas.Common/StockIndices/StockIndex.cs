namespace Smidas.Common.StockIndices
{
    public enum StockIndex
    {
        TestIndex = -1,

        #region Nordics

        [AffarsVarldenInfo(
            StockIndexUrl = "https://www.affarsvarlden.se/bors/kurslistor/stockholm-large/kurs/",
            StockIndicatorsUrl = "https://www.affarsvarlden.se/bors/kurslistor/stockholm-large/aktieindikatorn/")]
        [DagensIndustriInfo(Url = "https://www.di.se/bors/aktier/?Countries=SE&Lists=4&Lists=&Lists=&Lists=&Lists=&RootSectors=&RootSectors=")]
        OMXStockholmLargeCap = 0,

        [DagensIndustriInfo(Url = "https://www.di.se/bors/aktier/?Countries=DK&Lists=2&Lists=&Lists=&RootSectors=&RootSectors=")]
        OMXCopenhagenLargeCap = 1,

        [DagensIndustriInfo(Url = "https://www.di.se/bors/aktier/?Countries=FI&Lists=&Lists=&Lists=3&Lists=&RootSectors=&RootSectors=")]
        OMXHelsinkiLargeCap = 2,

        [DagensIndustriInfo(Url = "https://www.di.se/bors/aktier/?Countries=NO&Lists=&Lists=92580&Lists=&RootSectors=&RootSectors=")]
        OsloOBX = 3,

        #endregion

        #region Europe

        HDAX = 4,

        #endregion

        #region North America

        Nasdaq100AndSnP100 = 5,

        TSX60 = 6,

        #endregion
    }
}
