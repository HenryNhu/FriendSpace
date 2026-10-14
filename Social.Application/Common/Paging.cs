namespace Social.Application.Common;

public static class Paging
{
    public static int Skip(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 50)
            throw new ArgumentException("Số trang phải từ 1; số mục mỗi trang phải từ 1 đến 50.");
        var skip = ((long)pageNumber - 1) * pageSize;
        if (skip > int.MaxValue) throw new ArgumentException("Số trang quá lớn.");
        return (int)skip;
    }
}
public sealed class InvalidMediaException(string message) : ArgumentException(message);
public sealed class FriendshipExistsException() : Exception("Đã có lời mời hoặc quan hệ bạn bè với người này.");
