using System.Xml.Serialization;
using System.Collections.Generic;

namespace HPI.BBB.Autoscaler.Models.BBB
{
	[XmlRoot(ElementName = "attendee")]
	public class Attendee
	{
		[XmlElement(ElementName = "userID")]
		public string UserID { get; set; }
		[XmlElement(ElementName = "fullName")]
		public string FullName { get; set; }
		[XmlElement(ElementName = "role")]
		public string Role { get; set; }
		[XmlElement(ElementName = "isPresenter")]
		public string IsPresenter { get; set; }
		[XmlElement(ElementName = "isListeningOnly")]
		public string IsListeningOnly { get; set; }
		[XmlElement(ElementName = "hasJoinedVoice")]
		public string HasJoinedVoice { get; set; }
		[XmlElement(ElementName = "hasVideo")]
		public string HasVideo { get; set; }
		[XmlElement(ElementName = "clientType")]
		public string ClientType { get; set; }
	}

	[XmlRoot(ElementName = "attendees")]
	public class Attendees
	{
		[XmlElement(ElementName = "attendee")]
		public List<Attendee> Attendee { get; set; }
	}

	[XmlRoot(ElementName = "metadata")]
	public class Metadata
	{
		[XmlElement(ElementName = "bbb-origin-version")]
		public string Bbboriginversion { get; set; }
		[XmlElement(ElementName = "bbb-origin-server-name")]
		public string Bbboriginservername { get; set; }
		[XmlElement(ElementName = "bbb-origin")]
		public string Bbborigin { get; set; }
		[XmlElement(ElementName = "gl-listed")]
		public string Gllisted { get; set; }
	}

	[XmlRoot(ElementName = "meeting")]
	public class Meeting
	{
		[XmlElement(ElementName = "meetingName")]
		public string MeetingName { get; set; }
		[XmlElement(ElementName = "meetingID")]
		public string MeetingID { get; set; }
		[XmlElement(ElementName = "internalMeetingID")]
		public string InternalMeetingID { get; set; }
		[XmlElement(ElementName = "createTime")]
		public string CreateTime { get; set; }
		[XmlElement(ElementName = "createDate")]
		public string CreateDate { get; set; }
		[XmlElement(ElementName = "voiceBridge")]
		public string VoiceBridge { get; set; }
		[XmlElement(ElementName = "dialNumber")]
		public string DialNumber { get; set; }
		[XmlElement(ElementName = "attendeePW")]
		public string AttendeePW { get; set; }
		[XmlElement(ElementName = "moderatorPW")]
		public string ModeratorPW { get; set; }
		[XmlElement(ElementName = "running")]
		public string Running { get; set; }
		[XmlElement(ElementName = "duration")]
		public string Duration { get; set; }
		[XmlElement(ElementName = "hasUserJoined")]
		public string HasUserJoined { get; set; }
		[XmlElement(ElementName = "recording")]
		public string Recording { get; set; }
		[XmlElement(ElementName = "hasBeenForciblyEnded")]
		public string HasBeenForciblyEnded { get; set; }
		[XmlElement(ElementName = "startTime")]
		public string StartTime { get; set; }
		[XmlElement(ElementName = "endTime")]
		public string EndTime { get; set; }
		[XmlElement(ElementName = "participantCount")]
		public long ParticipantCount { get; set; }
		[XmlElement(ElementName = "listenerCount")]
		public long ListenerCount { get; set; }
		[XmlElement(ElementName = "voiceParticipantCount")]
		public long VoiceParticipantCount { get; set; }
		[XmlElement(ElementName = "videoCount")]
		public long VideoCount { get; set; }
		[XmlElement(ElementName = "maxUsers")]
		public long MaxUsers { get; set; }
		[XmlElement(ElementName = "moderatorCount")]
		public long ModeratorCount { get; set; }
		[XmlElement(ElementName = "attendees")]
		public Attendees Attendees { get; set; }
		[XmlElement(ElementName = "metadata")]
		public Metadata Metadata { get; set; }
		[XmlElement(ElementName = "isBreakout")]
		public string IsBreakout { get; set; }
	}

	[XmlRoot(ElementName = "meetings")]
	public class Meetings
	{
		[XmlElement(ElementName = "meeting")]
		public List<Meeting> Meeting { get; set; }
	}

	[XmlRoot(ElementName = "response")]
	public class GetMeetingsResponse
	{
		[XmlElement(ElementName = "returncode")]
		public string Returncode { get; set; }
		[XmlElement(ElementName = "meetings")]
		public Meetings Meetings { get; set; }
	}
}