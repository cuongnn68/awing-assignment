import { createBrowserRouter } from "react-router-dom"
import App from "./routes/App"
import { TreasureMap } from "./routes/TreasureMap"

export const urls = {
  home: "/",
  treasureMap: "/treasure-map"
}

export const router = createBrowserRouter([
  {
    path: urls.home,
    element: <App />,
  },
  {
    path: urls.treasureMap,
    element: <TreasureMap />
  }
])