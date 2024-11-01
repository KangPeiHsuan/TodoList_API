## 復刻一個 RESTFUL API 來管理 todos 列表
> 參考 https://todoo.5xcamp.us/api-docs/index.html 的 API 文件
 
### 實現功能
### Users
| 方法     | 路由 | 描述  |
| --- | --- | --- |
| POST    | /users/sign_in | 使用者登入 |
| DELETE  | /users/sign_out | 使用者登出 |

### Todos
| 方法     | 路由 | 描述  |
| --- | --- | --- |
| GET    | /todos | 取得 TODO 列表 |
| POST    | /todos | 新增 TODO |
| PUT  | /todos/{id} | 修改 TODO |
| DELETE  | /todos/{id} | 刪除 TODO |
| PATCH  | /todos/{id}/toggle | TODO 完成 / 已完成切換 |

### 專案的運行方式
### 專案結構
### 實作思路
### 遇到的挑戰和解決方案

### 實作筆記
[[note] Todos API 實作](https://hackmd.io/@kangpei/HJ_YC3hlJl)