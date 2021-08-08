# Smidas

A dockerized service for scraping and analyzing stock data using the AktieREA method.
The service is invokable through an HTTP-endpoint, which then does the scraping,
the analysis and the export of the data to Excel in a specified folder.

## Usage

`GET /BatchJob/<YourIndex>`

Say you've defined an index in `appsettings.json` as "NYSE", then you call `GET /BatchJob/NYSE`.

### appsettings.json

App behaviour is mainly configured through the appsettings.json file. It has two
main sections:
- AppSettings
- Serilog

Serilog is standard logging configuration, see Serilog configuration for further 
instructions.

AppSettings covers all application configuration and contains only ScrapingSets.
You can define multiple ScrapingSets and call them through the endpoint by the 
name you assign them, like in the example above.

### Appsettings Configuration Reference

#### Index
Setting | Description | Type | Default 
--- | --- | --- | --- 
AmountToBuy | Max amount of stock to buy | integer | 10
AmountToKeep | Max amount of stock to hold | integer | 10
CurrencyCode | CurrencyCode of your index | string | USD
ExportDirectory | Export location of Excel file | string | ~
IndexUrls | List of URLs to scrape index data | List<string> | N/A
AnalysisOptions | Options for controlling AktieREA | AnalysisOptionsObject | N/A
XPathExpressions | XPath values for finding data to scrape | XPathExpressionsObject | N/A
Industries | Dictionary of IndustryObjects for metadata on which stock belong to which industry. The key denotes industry name | Dictionary<string, IndustryObject> | N/A

#### AnalysisOptionsObject
Setting | Description | Type | Default
--- | --- | --- | ---
ExcludeNegativeProfitStocks | Whether or not to exclude stocks with negative profit | boolean | true
ExcludeZeroDividendStocks | Whether or not to exclude non-dividend stocks | boolean | true
ExcludePreferentialStocks | Whether or not to exclude preferential stocks | boolean | true

#### XPathExpressionsObject
Setting | Description | Type | Default
--- | --- | --- | ---
Names | XPath for stock names | string | N/A
Prices | XPath for stock prices | string | N/A
Volumes | XPath for stock volume | string | N/A
ProfitPerStock | XPath for stock's Profit/Stock value | string | N/A
AdjustedEquityPerStock | XPath for stock's adjusted equity/stock value | string | N/A
DirectDividend | XPath for stock's direct dividend value | string | N/A

#### IndustryObject
Setting | Description | Type | Default
--- | --- | --- | ---
Cap | Maximum amount of stocks to buy or hold within this industry | integer | 2
Companies | Name list of companies that belong to this industry | List<string> | N/A

