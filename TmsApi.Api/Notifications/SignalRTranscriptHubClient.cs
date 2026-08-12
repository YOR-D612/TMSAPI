using Microsoft.AspNetCore.SignalR;
using TmsApi.Application.Hubs;

namespace TmsApi.Api.Hubs;

public class SignalRTranscriptHubClient(
    IHubContext<TmsHub, ITmsHubClient> hubContext)
    : ITmsHubClient
{
    public Task ReceiveTranscriptReady(
        string reportId,
        string downloadUrl)
    {
        return hubContext.Clients.All.ReceiveTranscriptReady(
            reportId,
            downloadUrl);
    }

    public Task ReceiveCourseUpdate(
        string courseCode,
        string message)
    {
        return hubContext.Clients.All.ReceiveCourseUpdate(
            courseCode,
            message);
    }

    public Task ReceiveGradePosted(
        string courseCode,
        int studentId,
        decimal grade)
    {
        return hubContext.Clients.All.ReceiveGradePosted(
            courseCode,
            studentId,
            grade);
    }
}