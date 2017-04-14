namespace Wedding.Api.Business.Requests
{
    public class ParticipationRequest
    {
		public int UserId { get; set; }

		public string Street { get; set; }

		public string Number { get; set; }

		public string Box { get; set; }

		public int ZipCode { get; set; }

		public string City { get; set; }

		public string AdditionnalInfos { get; set; }

		public AttendingRequest[] Attendings { get; set; }
	}

	public class AttendingRequest
	{
		public int EventId { get; set; }
		
		public bool? IsAttending { get; set; }
	}
}
