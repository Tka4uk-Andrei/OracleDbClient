-- get parametrs for connection establishing
SELECT host_name FROM v$instance;
SELECT value FROM v$parameter WHERE name like '%service_name%';