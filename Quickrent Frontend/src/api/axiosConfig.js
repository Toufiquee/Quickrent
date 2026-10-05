import axios from "axios";
import { urlConfig } from "../configs/UrlConfig";

const apiClient = axios.create({
  baseURL: `${urlConfig.APP_URL}:${urlConfig.APP_PORT}`,
  headers: {
    "Content-Type": "application/json",
  },
});

apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("JwtToken");
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export default apiClient;
