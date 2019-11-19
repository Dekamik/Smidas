namespace Smidas.Common.StockIndices
{
    public enum StockIndex
    {
        #region Northern Europe

        [AffarsVarldenInfo(StockIndexUrl = "https://www.affarsvarlden.se/bors/kurslistor/stockholm-large/kurs/", StockIndicatorsUrl = "https://www.affarsvarlden.se/bors/kurslistor/stockholm-large/aktieindikatorn/")]
        [DagensIndustriInfo(Url = "https://www.di.se/bors/aktier/?Countries=SE&Lists=4&Lists=&Lists=&Lists=&Lists=&RootSectors=&RootSectors=")]
        OmxStockholmLargeCap = 0,

        [AffarsVarldenInfo(StockIndexUrl = "https://www.affarsvarlden.se/bors/kurslistor/danmark-large/kurs/", StockIndicatorsUrl = "https://www.affarsvarlden.se/bors/kurslistor/danmark-large/aktieindikatorn/")]
        [DagensIndustriInfo(Url = "https://www.di.se/bors/aktier/?Countries=DK&Lists=2&Lists=&Lists=&RootSectors=&RootSectors=")]
        OmxCopenhagenLargeCap = 1,

        [AffarsVarldenInfo(StockIndexUrl = "https://www.affarsvarlden.se/bors/kurslistor/finland-large/kurs/", StockIndicatorsUrl = "https://www.affarsvarlden.se/bors/kurslistor/finland-large/aktieindikatorn/")]
        [DagensIndustriInfo(Url = "https://www.di.se/bors/aktier/?Countries=FI&Lists=&Lists=&Lists=3&Lists=&RootSectors=&RootSectors=")]
        OmxHelsinkiLargeCap = 2,

        [DagensIndustriInfo(Url = "https://www.di.se/bors/aktier/?Countries=NO&Lists=&Lists=92580&Lists=&RootSectors=&RootSectors=")]
        OsloObx = 3,

        #endregion

        #region North America

        [DagensIndustriInfo(Url = "https://www.di.se/bors/aktier/?Countries=US&Lists=&Lists=46295&RootSectors=&RootSectors=")]
        Nasdaq = 4,

        [DagensIndustriInfo(Url = "https://www.di.se/bors/aktier/?Countries=US&Lists=&Lists=&Lists=&Lists=&Lists=71213&RootSectors=&RootSectors=")]
        NewYorkStockExchange = 5,

        SNP500 = 6,

        #endregion
    }
}
