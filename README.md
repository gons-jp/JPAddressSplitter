# JPAddressSplitter
日本郵政のWEBサイトからダウンロードできるCSVファイル「KEN_ALL.CSV」を使用して
日本の住所を、「都道府県」、「市区郡町村」、「町域」、「番地」、「建物情報」の5つに分割します。

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
```

