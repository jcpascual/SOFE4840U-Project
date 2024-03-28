# Conference

Adjust the connection string in `Conference/appsettings.Development.json` to point to a MariaDB server.

To enable encryption, [set up the MariaDB server according to the user manual](https://mariadb.com/kb/en/data-at-rest-encryption-overview/) and add `ENGINE=InnoDB ENCRYPTED=YES` to the queries in `Conference/CreateTable.sql`.
