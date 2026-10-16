import type { Language } from '../../shared/i18n/messages';

interface FeedMessages {
  personal: string; public: string; newPosts: string; showNew: string;
  loadMore: string; end: string; emptyPersonal: string; emptyPublic: string;
  findFriends: string; noFriends: string; pollPaused: string; refreshFailed: string;
}

export const feedMessages: Record<Language, FeedMessages> = {
  vi: {
    personal: 'Bài viết của bạn và bạn bè, theo quyền xem của từng bài.',
    public: 'Bài viết công khai từ cộng đồng FriendSpace.',
    newPosts: 'Có bài viết mới.', showNew: 'Xem ngay', loadMore: 'Xem thêm',
    end: 'Bạn đã xem hết các bài hiện có trong bảng tin.',
    emptyPersonal: 'Chưa có bài viết nào. Hãy chia sẻ bài đầu tiên hoặc tìm thêm bạn bè.',
    emptyPublic: 'Chưa có bài viết công khai.', findFriends: 'Tìm bạn',
    noFriends: 'Bạn chưa có bạn bè được xác nhận. Bảng tin hiện chỉ có bài của bạn.',
    pollPaused: 'Đã tạm dừng kiểm tra bài mới. Nhấn Tải lại để thử kết nối lại.',
    refreshFailed: 'Chưa tải lại được bảng tin. Nội dung đang hiển thị có thể chưa cập nhật.',
  },
  en: {
    personal: 'Posts from you and your friends, subject to each post’s visibility.',
    public: 'Public posts from the FriendSpace community.',
    newPosts: 'New posts are available.', showNew: 'Show posts', loadMore: 'Load more',
    end: 'You have reached the end of the available feed.',
    emptyPersonal: 'No posts yet. Share your first post or find more friends.',
    emptyPublic: 'No public posts yet.', findFriends: 'Find people',
    noFriends: 'You have no confirmed friends yet. This feed currently shows only your posts.',
    pollPaused: 'New-post checks are paused. Select Refresh to reconnect.',
    refreshFailed: 'The feed could not be refreshed. Displayed posts may be out of date.',
  },
};
