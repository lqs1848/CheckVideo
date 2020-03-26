# CheckVideo
Test if video is playable 测试视频是否可播放


![image](https://github.com/lqs1848/CheckVideo/blob/master/info/1.jpg)<br>

调用ffmpeg 检测视频文件是否可以播放<br/>
只是快速检测<br/>
截取视频的第一帧<br/>
能截到就是可以播放<br/>
ffmpeg无法播放的视频会误判<br/>
打开超时的也会判定为无法播放<br/>
