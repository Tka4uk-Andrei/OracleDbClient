﻿SELECT SALES.ID, GOODS.NAME, SALES.GOOD_COUNT, to_char(SALES.CREATE_DATE, 'DD-MM-YYYY HH24:MI:SS')as Sale_Date FROM SALES, GOODS
    WHERE GOODS.ID = SALES.GOOD_ID
    ORDER BY Sale_Date DESC, GOODS.NAME;

INSERT INTO SALES (GOOD_ID, GOOD_COUNT, CREATE_DATE) 
    VALUES ( (SELECT MAX(ID) FROM GOODS WHERE NAME = 'book_1') , 3, CURRENT_TIMESTAMP);

UPDATE WAREHOUSE2 SET good_count = 100 where good_id = (SELECT MAX(id) FROM GOODS WHERE NAME = 'alter');

DELETE FROM WAREHOUSE2 WHERE GOOD_ID = (SELECT MAX(id) FROM GOODS WHERE NAME = '1');