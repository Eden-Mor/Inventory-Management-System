# IMS
Inventory Management System was created to allow a given business to easily manage their stock, and sell it to a consumer.

## Prerequisites 
Install docker desktop on the machine that will host the site (https://www.docker.com/products/docker-desktop/)

## LIVE Setup
1. To setup the app for yourself, clone the respository.
2. In the IMS-Backend folder, rename appsettings-EXAMPLE.json to appsettings.json and make the changes for your business.
3. Go to the IMS-Backend/Templates folder and make your receipt with html (you may also have a logo.png file to "insert" an image into the template, optionally just use base64 directly in the template)
4. Optionally you may change the port that is used to host the site inside docker-compose.override.yml, by default it is 6565
5. Run Docker-Helper.bat
6. Hit "Enter" key until you reach step 7
7. If this is your first time running the app OR there have been updates to the database
    When asked "Optional: enter a service command to run (migrate-db), or press Enter to skip:"
    Put in "migrate-db" without the quotation marks
    IMPORTANT: It is recommended you make a backup of the database files
8. Continue hitting "Enter"
9. Wait until the containers have finished
10. You can check whether the process has completed successfully by opening Docker Desktop and checking the container statuses.


## Using the app
If using the local version (from the same machine this is hosted on)
Navigate to "http://localhost:6565/"

If this app is hosted on another machine on the network, grab the IPV4 ip of the hosting computer and then navigate to
YOUR.LOCAL.IPV4.IP:6565

If you want to host it publicly you would need to use something like traefik, which this post will not go over.

The app does not have authentication at this time, so I recommend using firewall rules to allow/disallow certain ip's from accessing the site.


## Updating the app
1. Grab the latest code from the repository
2. Copy over the files appsettings.json from IMS-Backend and any templates you made in IMS-Backend/Templates to the new code
3. Follow steps 5-10 from the LIVE Setup section

## Making a backup of the database
1. Stop the containers from running using Docker Desktop (or the Docker-Helper.bat))
2. Create a folder in C:\IMS-Backup (or anywhere you want)
3. Open command prompt and run the following command:
   ``` 
   docker run --rm -v ims_pgdata:/volume -v C:/IMS-Backup:/backup alpine sh -c "tar czf /backup/pgdata.tar.gz -C /volume ."
   ```
   Note: if you put the backup folder somewhere else, change the path accordingly in the command above