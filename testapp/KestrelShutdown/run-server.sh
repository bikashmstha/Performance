#!/bin/bash

dotnet build
while true
do
  dotnet run --framework netcoreapp1.1
done
