#!/bin/bash
# 验证云服务器上 Review/Archive/GetProjectValidations 路由是否存在
set +e
BASE=http://localhost:8957

HASH=$(printf '%s' 'admin' | openssl dgst -sha256 -binary | base64)
RESP=$(curl -s "$BASE/api/User/AccountLogin?userName=admin&password=${HASH}&version=1&hasProcess=0")
TOKEN=$(echo "$RESP" | grep -o '"TokenValue":"[^"]*"' | head -1 | cut -d'"' -f4)
echo "LOGIN: $(echo "$RESP" | head -c 60)"
if [ -z "$TOKEN" ]; then
  echo "登录失败，无法获取 Token"
  exit 1
fi

for path in \
  "/api/Project/GetProjectValidations?projectId=00000000-0000-0000-0000-000000000000" \
  "/api/Review/GetSubmissions?scope=pending" \
  "/api/Review/GetHistory?projectId=00000000-0000-0000-0000-000000000000" \
  "/api/Archive/GetCandidates" \
  "/api/Archive/GetArchives" \
  "/api/Project/NotExist" ; do
  echo "== $path =="
  curl -s -w "\nHTTP:%{http_code}\n" "$BASE$path" -H "UserId: 1" -H "Token: $TOKEN"
  echo
done