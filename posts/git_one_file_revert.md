# How to revert file to given commit in git

If you want to revert one file to particulat commit then it would be good to have a list of commit that touched that file. To dod that please use following command:

```
git log -p -- file_name
```

Then you have to use commit it (hash) to specify to which point in time you want to go back:

```
git checkout commit_it file_name
```