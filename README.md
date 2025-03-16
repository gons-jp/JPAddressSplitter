# JPAddressSplitter
日本郵政のWEBサイトからダウンロードできるCSVファイル「KEN_ALL.CSV」を使用して

日本の住所を、「都道府県」、「市区郡町村」、「町域」、「番地」、「建物情報」の5つに分割します。
分割の際に「ツ」「ケ」「ノ」等の大小も日本郵政のデータを元に修正して返しますので
配送会社の送り状データ作成時に通りやすくなるようになっています。

[KEN_ALL.CSV](https://www.post.japanpost.jp/zipcode/dl/kogaki-zip.html)(全国一括からダウンロード)

使用方法
```
JPAddressSplitter.AddressSpliter ast = 
  new JPAddressSplitter.AddressSpliter("c:\\temp\\KEN_ALL.CSV");

var addressList = ast.Split("東京都港区芝公園４丁目２-８東京タワー");
foreach(JPAddressSplitter.SplitInfo si in addressList)
{
  Console.WriteLine(si.AddressType.ToString() + ":" + si.AddressValue);
}

//TypTodofuken:東京都
//TypShikuchoson:港区
//TypChoiki:芝公園
//TypBanchi:４丁目２-８
//TypTatemono:東京タワー
```
