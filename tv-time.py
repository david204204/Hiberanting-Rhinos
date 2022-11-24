import os
from subprocess import *
from threading import Thread, Lock
import time
from dotenv import find_dotenv, load_dotenv
# the env contain the real name of the c# app for better scalability and protection
dotenv_path = find_dotenv()
load_dotenv(dotenv_path)
GET_TVSHOW_TOTAL_LENGTH_BIN = os.getenv('GET_TVSHOW_TOTAL_LENGTH_BIN')
# lock for threading protect from run over each other
mutex = Lock()
# global var for count num of finished threads
finished_count = 0

# read the files from the txt file
def file_reading():
    file = open('tv-shows.txt', 'r')
    read = file.readlines()
    tvshowlist = []
    for line in read:
        tvshowlist.append(line[:-1])

    return tvshowlist


def start_process(show, mutex, run_time, name_show):
    # running the c# app with one show name argument at the time, for letting us the ability
    # to create for each show-name a thread
    p1 = run(f"{GET_TVSHOW_TOTAL_LENGTH_BIN} {show}", capture_output=True, text=True).returncode
    # protect the data output from the c# app
    mutex.acquire()
    run_time.append(int(p1))
    name_show.append(show)
    global finished_count
    finished_count += 1
    mutex.release()



def main():
    shows = file_reading()
    mutex = Lock()
    run_time = []
    name_show = []
    for i in range(len(shows)):
        show = shows[i]
        # make threads for each func for more speed
        Thread(target=start_process, args=(show, mutex, run_time, name_show)).start()
        # wait for c# app finished for avoiding running over threads
        time.sleep(0.15)
    # waiting for all threads finish their process for release and continue the rest
    while True:
        mutex.acquire()
        if finished_count == len(shows):
            break
        mutex.release()
        time.sleep(0.15)
    minimum_num = min(run_time)
    maximum_num = max(run_time)
    index_of_min = run_time.index(minimum_num)
    index_of_max = run_time.index(maximum_num)
    # if the c# app not found the tv shows, it will display the following massage
    if minimum_num <= 10:
        print(f'Could not get info for: {name_show[index_of_min]}')
        print(f'The longest show: {name_show[index_of_max]} ({maximum_num / 60}h {maximum_num % 60}m)')
    # if the c# app succeed, the longest and the shortest will display
    else:
        print(f'The longest show: {name_show[index_of_max]} ({maximum_num / 60}h {maximum_num % 60}m)')
        print(f'The shortest show: {name_show[index_of_min]} ({minimum_num / 60}h {minimum_num % 60}m)')


if __name__ == '__main__':
    main()
