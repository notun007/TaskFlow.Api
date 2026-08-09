using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Domain.Tests.Tasks;

public sealed class TaskAttachmentTests
{
    [Fact]
    public void Attachment_keeps_file_metadata_and_content()
    {
        var taskId = Guid.NewGuid();
        var content = new byte[] { 1, 2, 3 };
        var attachment = new TaskAttachment
        {
            TaskItemId = taskId,
            FileName = "evidence.pdf",
            ContentType = "application/pdf",
            Size = content.Length,
            Content = content,
            UploadedBy = "owner@example.com"
        };

        Assert.Equal(taskId, attachment.TaskItemId);
        Assert.Equal("evidence.pdf", attachment.FileName);
        Assert.Equal(content, attachment.Content);
        Assert.Equal(3, attachment.Size);
    }

    [Fact]
    public void Task_can_collect_attachments()
    {
        var task = new TaskItem { TaskNumber = "TF-1", Title = "Investigate incident", Type = "Task" };
        task.Attachments.Add(new TaskAttachment { FileName = "notes.txt", ContentType = "text/plain", Size = 4, Content = "test"u8.ToArray(), UploadedBy = "analyst" });

        Assert.Single(task.Attachments);
        Assert.Equal("notes.txt", task.Attachments.Single().FileName);
    }
}
