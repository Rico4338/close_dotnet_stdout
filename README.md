# [dotnet core]為何會需要於container關閉stdout
當我們今天運行dotnet debug於本機環境的時候，很習慣會開著console視窗或是相關的輸出視窗於畫面上，人只要盯著輸出視窗就可以看到相關對應的資訊輸出，對於debug與相關的log是很方便的。

![image](https://github.com/Rico4338/close_dotnet_stdout/blob/main/image/console_log.jpg?raw=true)

但是今天當把程式deploy到container環境上的時候，千萬小心這麼做，因為這些log都會被docker捕捉下來放進硬碟中，也是因為要讓每次下docker logs 可以看到相關紀錄也才要儲存。

正常來說，docker的log是放在下面的路徑裡
```
/var/lib/docker/containers/<container-id>/<container-id>-json.log
```

然後我簡單寫了一個每秒送log的小code

``` c share
Task.Run(async () =>
{
    while (true)
    {
        _logger.LogInformation(DateTime.Now.ToString("yyyy-M-d dddd HH:mm:ss"));
        await Task.Delay(1000);
    }
});
```
在Program.cs有加入
``` c share
.ConfigureLogging((hostingContext, logging) =>
{
    //在container環境要記得把這的log關掉
    logging.AddConsole();
})
```
然後因為測試需求，我特別build了一個image來跑它，所以需要先在該目錄下執行
``` bash
docker build -t dotnet-5.0-run .
```
之後就可以直接run docker-compose
``` bash
docker-compose up -d
```
這個時候若是containe有正確跑起來，下docker logs [container id] 應該就會看到如下的話面

![image](https://github.com/Rico4338/close_dotnet_stdout/blob/main/image/1.png?raw=true)

之後我們進入docker host的docker路徑下找log對應的檔案

``` bash
sudo cd /var/lib/docker/containers/<container-id>/
```

![image](https://github.com/Rico4338/close_dotnet_stdout/blob/main/image/2.png?raw=true)

從上圖的兩個圈圈中發現檔案是有在增長的，而且放著不動它就是會一直肥下去，直到你的硬碟滿了，那之後就可能發生docker host crash導致服務都停擺，甚至要整個重開機都有可能。

所以這個問題會隨container活的越久越會容易發生，反正log平時都會落地在相關的檔案裡，因此像是這一類stdout的log就把它關掉吧，省點io資源浪費。

※或是參考[stackoverflow](https://stackoverflow.com/questions/31829587/docker-container-logs-taking-all-my-disk-space)增加設定
```
--log-opt max-size=50m 
```
