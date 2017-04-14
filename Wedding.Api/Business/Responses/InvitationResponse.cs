using System;
using Wedding.Api.Business.Databases;

namespace Wedding.Api.Business.Responses
{
    public class InvitationResponse
    {
	    public InvitationResponse()
	    {
		    
	    }

	    public InvitationResponse(InvitationView participation)
	    {
		    UserId = participation.UserId;
		    IsAttending = participation.IsAttending;
		    EventId = participation.EventId;
		    EventName = participation.EventName;
		    EventDescription = participation.EventDescription;
		    StartDate = DateTime.SpecifyKind(participation.StartDate, DateTimeKind.Local);
		    PlaceId = participation.PlaceId;
		    PlaceName = participation.PlaceName;
		    Description = participation.Description;
		    Url = participation.Url;
		    Latitude = participation.Latitude;
		    Longitude = participation.Longitude;
		    Street = participation.Street;
		    Number = participation.Number;
		    ZipCode = participation.ZipCode;
		    City = participation.City;
	    }

		public int UserId { get; set; }

		public bool? IsAttending { get; set; }

		public int EventId { get; set; }

		public string EventName { get; set; }

		public string EventDescription { get; set; }

		public DateTime StartDate { get; set; }

		public int PlaceId { get; set; }

		public string PlaceName { get; set; }

		public string Description { get; set; }

		public string Url { get; set; }

		public decimal Latitude { get; set; }

		public decimal Longitude { get; set; }

		public string Street { get; set; }

		public string Number { get; set; }

		public int ZipCode { get; set; }

		public string City { get; set; }
	}
}
