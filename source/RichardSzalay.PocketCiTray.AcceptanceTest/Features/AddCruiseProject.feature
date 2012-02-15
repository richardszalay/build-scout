Feature: Add Cruise Project
	In order to know how my jobs are doing
	As a developer
	I want to monitor the state of a cruise job

Scenario: Add Successful
	Given I have a build server called "buildServer"
	And the build server has a job called "job1"
	And my app is clean installed and running
	When I add a new build server "buildServer"
	And I add the "job1" job

@Ignore
Scenario: Failed
	Given I have a build server called "buildServer"
	And the build server has a job called "job1"
	And the job's last run failed
	And I am monitoring the job
	When I view the status of my jobs
	Then the result should indicate the job failed

@Ignore
Scenario: Unavailable
	Given I have a build server called "buildServer"
	And the build server has a job called "job1"
	And the build server is unavailable
	And I am monitoring the job
	When I view the status of my jobs
	Then the result should indicate the job is unavailable
