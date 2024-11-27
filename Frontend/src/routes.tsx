import {createBrowserRouter} from "react-router"
import LoginPage from "./pages/LoginPage.tsx"

const router = createBrowserRouter([
    {
        path: "/",
        element: <LoginPage />,
    },
    
])
export default router