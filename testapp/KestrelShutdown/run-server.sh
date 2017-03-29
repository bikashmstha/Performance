#!/bin/bash

dotnet build
while true
do
  dotnet run --framework netcoreapp2.0
done
