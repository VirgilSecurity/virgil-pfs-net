# Generic build script that builds, tests, and creates nuget packages.
# 
# INSTRUCTIONS:
#	Update the following project paths:
#		proj		Path to the project file (.csproj)
#		test		Path to the test project (.csproj)
#		nuspec		Path to the package definition for NuGet.
#		
#		delete any of the lines if not applicable
#
#
#	Update the paths to the build tools:
#		msbuild		Path to msbuild
#		test		Path to mstest.exe (requires Visual Studio) (optional - delete line)
#		nuget		Path to nuget.exe (requires NuGet command line tool) (optional - delete line)
#		trx2html	Path to trx2html.exe (http://trx2html.codeplex.com/) (optional - delete line)
#

import os, shlex, subprocess, re, datetime	
		
class MsBuilder:
	def __init__(self, msbuild=None, mstest=None, nuget=None, trx2html=None):
		# The following dictionary holds the location of the various
		#	msbuild.exe paths for the .net framework versions
		if msbuild==None:
			self.msbuild = r'C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe'
		else:
			self.msbuild = msbuild
			
		# Path to mstest (this requires vs2010 to be installed
		if mstest==None:
			self.mstest = r'C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\MSTest.exe'
		else:
			self.mstest = mstest
		
		# Path to nuget packager
		if nuget==None:
			self.nuget = r'./SessionStorageLib/NuGet.exe'
		else:
			self.nuget = nuget
		
	def build(self, projPath):
		# Ensure msbuild exists
		if not os.path.isfile(self.msbuild):
			raise Exception('MsBuild.exe not found. path=' + self.msbuild)

		args = [self.msbuild, projPath, '/t:Rebuild', '/p:Configuration=Release']

		p = subprocess.call(args)
		if p==1: return False	# exit early
		
		return True
		
	def pack(self, projPath, outputPath):
			
		if not os.path.exists(outputPath):
			os.makedirs(outputPath)
			
		p = subprocess.call([self.nuget, 'pack', projPath, '-Prop', 'Configuration=Release', '-o', outputPath])
			
		if p==1: return False #exit early
		
		return True
		
	def run(self, proj=None, test=None, nuspec=None):
		summary = '';
		
		# File header	
		start = datetime.datetime.now()
		print('\n'*5)
		summary += self.log('STARTED BUILD - ' + start.strftime("%Y-%m-%d %H:%M:%S"))

		# Build the project
		if proj is not None:
			buildOk = self.build(proj)
			if not buildOk:
				self.log('BUILD: FAILED', start)
				sys.exit(100)
			summary += self.log('BUILD: SUCCEEDED', start)
		else:
			summary += self.log('BUILD: NOT SPECIFIED')
			
		# Build the tests and run them
		if test is not None:
			testOk = self.test(test)
			if not testOk:
				print(self.log('TESTS: FAILED', start))
				sys.exit(100)
			summary += self.log('TESTS: PASSED', start)
		else:
			summary += self.log('TESTS: NOT SPECIFIED')

		# Package up the artifacts
		if nuspec is not None:
			packOk = self.pack(nuspec, '0.0.0.0')
			if not packOk:
				print(self.log('NUGET PACK: FAILED', start))
				sys.exit(100)
			summary += self.log('NUGET PACK: SUCCEEDED', start)
		else:
			summary += self.log('NUGET PACK: NOT SPECIFIED')
			
		# Validate dependencies
		if not self.validate(proj):
			print(self.log('DEPENDENCIES: NOT VALIDATED - DETECTED UNVERSIONED DEPENDENCY', start))
			sys.exit(100)
		summary += self.log('DEPENDENCIES: VALIDATED', start)

		# Build footer
		stop = datetime.datetime.now()
		diff = stop - start
		summary += self.log('FINISHED BUILD', start)
		
		# Build summary
		print('\n\n' + '-'*80)
		print(summary)
		print('-'*80)
		
	def log(self, message, start=None):
		timestamp = ''
		numsecs = ''
		if start is not None:
			split = datetime.datetime.now()
			diff = split - start
			timestamp = split.strftime("%Y-%m-%d %H:%M:%S") + '\t'
			numsecs = ' (' + str(diff.seconds) + ' seconds)'
		msg = timestamp + message + numsecs + '\n\n'
		print('='*10 + '> ' + msg)
		return msg