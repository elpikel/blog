# How to setup a test

Typically when we setup our system under test we use factories to do that. Which I thought is totally fine until I listened to [Saša Jurić on Training](https://podcast.smartlogic.io/s3e17-juric) where you can find beside other interesting thoughts the one about test setup. According to [Saša Jurić](https://www.theerlangelist.com) we should not use factories to setup system under test we should use exercised system in setup routine. Doing that we will actually test two things:

- if functions used in setup works as we think
- if tested functionality works as it should

This is true in most of the cases but not when we are testing migration logic because it depends on some particular state of database. We don't want to update this logic when our setup routine changes because migration is usually one time thing. If it isn't then we should use also application logic in test setup.
