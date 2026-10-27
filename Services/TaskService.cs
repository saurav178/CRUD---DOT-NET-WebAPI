using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TaskApi.Models;

namespace TaskApi.Services;

public class TaskService : ITaskService
{
    private readonly IMongoCollection<TaskItem> _tasksCollection;

    public TaskService(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
    {
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _tasksCollection = database.GetCollection<TaskItem>(settings.Value.CollectionName);
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _tasksCollection.Find(_ => true).ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(string id)
    {
        return await _tasksCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        await _tasksCollection.InsertOneAsync(task);
        return task;
    }

    public async Task<bool> UpdateAsync(string id, TaskItem updatedTask)
    {
        updatedTask.Id = id;
        var result = await _tasksCollection.ReplaceOneAsync(t => t.Id == id, updatedTask);
        return result.IsAcknowledged && result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _tasksCollection.DeleteOneAsync(t => t.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}
