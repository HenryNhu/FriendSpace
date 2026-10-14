import { BrowserRouter, Route, Routes } from 'react-router';
import { LanguageProvider } from '../shared/i18n/LanguageProvider';
import { ToastProvider } from '../shared/ui/toast/ToastProvider';
import { AuthProvider } from '../features/auth/AuthProvider';
import { RequireAuth } from '../features/auth/RequireAuth';
import { AppLayout } from './AppLayout';
import FeedPage from '../pages/feed/FeedPage';
import { AuthPage } from '../pages/auth/AuthPage';
import { ProfilePage } from '../pages/profiles/ProfilePage';
import { ProfileSettingsPage } from '../pages/profiles/ProfileSettingsPage';
import { PostDetailPage } from '../pages/posts/PostDetailPage';
import { PeoplePage } from '../pages/people/PeoplePage';
import { FriendsPage } from '../pages/friends/FriendsPage';
import { NotFoundPage } from '../pages/NotFoundPage';
export default function App() {
  return <LanguageProvider><ToastProvider><AuthProvider><BrowserRouter><AppLayout>
  <Routes>
   <Route path="/" element={<FeedPage />} />
   <Route path="/login" element={<AuthPage mode="login" />} />
   <Route path="/register" element={<AuthPage mode="register" />} />
   <Route path="/profiles/:userId" element={<ProfilePage />} />
   <Route path="/posts/:postId" element={<PostDetailPage />} />
   <Route path="/people" element={<PeoplePage />} />
   <Route element={<RequireAuth />}><Route path="/friends" element={<FriendsPage />} /><Route path="/settings/profile" element={<ProfileSettingsPage />} /></Route>
   <Route path="*" element={<NotFoundPage />} />
  </Routes>
 </AppLayout></BrowserRouter></AuthProvider></ToastProvider></LanguageProvider>;
}
