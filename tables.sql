-- Created by Vertabelo (http://vertabelo.com)
-- Last modification date: 2024-04-11 12:59:07.533

-- tables
-- Table: GroupAssignments
CREATE TABLE GroupAssignments (
                                  Students_ID int  NOT NULL,
                                  Groups_ID int  NOT NULL,
                                  CONSTRAINT GroupAssignments_pk PRIMARY KEY  (Students_ID,Groups_ID)
);

-- Table: Groups
CREATE TABLE Groups (
                        ID int  NOT NULL IDENTITY,
                        Name nvarchar(50)  NOT NULL,
                        CONSTRAINT Groups_pk PRIMARY KEY  (ID)
);

-- Table: Students
CREATE TABLE Students (
                          ID int  NOT NULL IDENTITY,
                          FirstName nvarchar(50)  NOT NULL,
                          LastName nvarchar(50)  NOT NULL,
                          Phone nvarchar(9)  NOT NULL,
                          Birthdate date  NOT NULL,
                          CONSTRAINT Students_pk PRIMARY KEY  (ID)
);

-- foreign keys
-- Reference: GroupAssignments_Groups (table: GroupAssignments)
ALTER TABLE GroupAssignments ADD CONSTRAINT GroupAssignments_Groups
    FOREIGN KEY (Groups_ID)
        REFERENCES Groups (ID);

-- Reference: GroupAssignments_Students (table: GroupAssignments)
ALTER TABLE GroupAssignments ADD CONSTRAINT GroupAssignments_Students
    FOREIGN KEY (Students_ID)
        REFERENCES Students (ID);

-- End of file.

