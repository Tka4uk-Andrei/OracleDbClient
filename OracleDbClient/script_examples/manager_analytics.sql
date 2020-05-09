create or replace PROCEDURE demand_count_by_name (start_time IN VARCHAR2, end_time IN VARCHAR2, good_name IN VARCHAR2, count_ out NUMBER)
IS
BEGIN
    select SUM(GOOD_COUNT) into count_ from SALES where GOOD_ID = (SELECT MAX(id) FROM GOODS WHERE name = good_name) and 
        CREATE_DATE between TO_TIMESTAMP(start_time, 'DD-MON-YY HH:MI:SS') and TO_TIMESTAMP(end_time, 'DD-MON-YY HH:MI:SS');
    if count_= null
    then
        count_:=0;
    end if;
END demand_count_by_name;