namespace Smidas.Common.StockIndices
{
    public enum StockIndex
    {
        #region Northern Europe

        [AffarsVarldenInfo(
            StockIndexUrl = "https://www.affarsvarlden.se/bors/kurslistor/stockholm-large/kurs/", 
            StockIndicatorsUrl = "https://www.affarsvarlden.se/bors/kurslistor/stockholm-large/aktieindikatorn/", 
            Currency = "SEK")]
        [DagensIndustriInfo(
            Url = "https://www.di.se/bors/aktier/?Countries=SE&Lists=4&Lists=&Lists=&Lists=&Lists=&RootSectors=&RootSectors=", 
            Currency = "SEK")]
        OmxStockholmLargeCap = 0,

        [DagensIndustriInfo(
            Url = "https://www.di.se/bors/aktier/?Countries=DK&Lists=2&Lists=&Lists=&RootSectors=&RootSectors=",
            Currency = "DKK")]
        OmxCopenhagenLargeCap = 1,

        [DagensIndustriInfo(
            Url = "https://www.di.se/bors/aktier/?Countries=FI&Lists=&Lists=&Lists=3&Lists=&RootSectors=&RootSectors=",
            Currency = "EUR")]
        OmxHelsinkiLargeCap = 2,

        [DagensIndustriInfo(
            Url = "https://www.di.se/bors/aktier/?Countries=NO&Lists=&Lists=92580&Lists=&RootSectors=&RootSectors=",
            Currency = "NOK")]
        OsloObx = 3,

        #endregion

        #region North America

        [DagensIndustriInfo(
            Url = "https://www.di.se/bors/aktier/?Countries=US&Lists=&Lists=46295&RootSectors=&RootSectors=",
            Currency = "USD")]
        [DagensIndustriInfo(
            Url = "https://www.di.se/bors/aktier/?Countries=US&Lists=&Lists=&Lists=&Lists=&Lists=71213&RootSectors=&RootSectors=",
            Currency = "USD")]
        SNP500 = 4,

        #endregion
    }
}
