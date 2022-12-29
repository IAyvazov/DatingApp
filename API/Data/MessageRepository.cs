using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context;

        public MessageRepository(DataContext context)
        {
            this.context = context;
        }

        public void AddMessage(Message message)
        => this.context.Messages.Add(message);

        public void DeleteMessage(Message message)
        => this.context.Messages.Remove(message);
    

        public async Task<Message> GetMessage(int id)
        => await this.context.Messages.FindAsync(id);

        public async Task<PagedList<MessageDto>> GetMessagesForUser()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(int currentUserId, int recipientId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SaveAllAsync()
        => await this.context.SaveChangesAsync() > 0;
    }
}