# External Tool Configuration

To add External Tools in the GFrind UI menu, place all executable and metadata files under GFrinedUILocation\tools

Metadata
--------
Metadata file MUST have extension of 'gftool' (ex. MeGustasTu.gftool)

Metadta file should XML formatted document like below:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<gftool>
  <executable>
    <!-- name of executable ex. MeGustasTu.exe -->
  </executable>
  <displayname>
    <!-- name which will be disaplyed in GFried UI menu -->
  </displayname>
  <description>
    <!-- Description of tools>
  </description>
  
  <!-- Following section will describe the command line arguments of tools -->
  <args>
	<!-- name: name of argument, mandatory: ture if it is mandatory argument -->
    <arg name ="Device Identifier" mandatory ="false">
      <switch>
		<!-- swith of command line argument (normally starts with '-') -->
	  </switch>
      <value>
	    <!-- value that need to be passed from GFrined UI -->
	  </value>
    </arg>
  </args>
</gftool>
```
