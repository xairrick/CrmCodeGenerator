Allows you to generate CRM 2011/2013 schema files based on a T4 template. 
Template is saved to project so that you can modify it as needed. 

Version 0.8.2
[UPDATE] saving originally generated code so that is can be re-used if the developer cancels the login process the code generation does wipe out the file
[REFACTOR] Cleaning up the code in DteHelper
[UPDATE] Made Intersect Entities (aka Native N:N) readonly in the standard template
[UPDATE] Changed the default URL to match CRM OnLine
[BUG] Fixed bug that was causing the setting to get lost (ie not saved in the .SLN)
[BUG] was defaulting to the last organization that was selected.
[UPDATE] Better error handling for connection problems
[BUG] fixed so that the ErrorList window would be cleared of any previous template errors
[UPDATE] Username & Password (encrypted) now stored in the .SUO file
[UPDATE] password input is now using a PasswordBox so the password is concealed.


Version 0.8.1
[UPDATE] updated wording of menu item
[BUG] Template was being added if the developer hit cancel or close.
[UPDATE] removed Min/Max button from dialogs
[BUG] Fixed Typo in Dialogbox
[UPDATE] Leaving Login window until have the mapping is done, so it clear to the developer that it's still working.
[UPDATE] Any syntax errors in the template will generate an Error and show the Error List

Version 0.8.0
Complete overhaul.   Menu item now only add the template with the 'Custom Tool' attached.  The 'Custom Tool'  will then prompt for login credentials