On classroom message{
	TimeSpan minutesToWait = new TimeSpan(0,5,0);
	if(message.ContainsTimeStamp()){
		waitUntil(message.timestamp - minutesToWait);
	}
	string link = defaultMeetLink;
	if(message.ContainsMeetLink()){
		link = message.link;
	}
	var now = DateTime.Now;
	while(now+minutesToWait > DateTime.Now){
		// Wait
	}
	enterMeet(link);
}
On classroom post{
	
}
