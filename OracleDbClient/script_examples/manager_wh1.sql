SELECT good_id, good_count FROM WAREHOUSE1;

UPDATE WAREHOUSE1 SET good_count = 10 where good_id = (SELECT MAX(id) FROM GOODS WHERE NAME = 'alter');

INSERT INTO WAREHOUSE1 (good_id, good_count) VALUES ((SELECT MAX(id) FROM GOODS WHERE NAME = 'alter'), 100);

delete from WAREHOUSE1 where GOOD_ID = (select max(ID) FROM GOODS where NAME = 'alter');