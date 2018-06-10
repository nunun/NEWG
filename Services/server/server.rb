require 'webrick'
require 'thread'

$server_cmd  = './Builds/Server'
$server_pid  = nil
$server_stop = 0
$m           = Mutex.new

webrick = WEBrick::HTTPServer.new({
  DocumentRoot:   './',
  BindAddress:    '0.0.0.0',
  Port:           17777,
})

webrick.mount_proc '/' do |req, res|
  res.body = server_running.to_s
end

webrick.mount_proc '/start' do |req, res|
  server_start
  res.body = "started"
end

webrick.mount_proc '/stop' do |req, res|
  server_stop
  res.body = "stopped"
end

def server_start
  $m.synchronize do
    if server_running
      return
    end
    $server_pid  = fork do exec $server_cmd end
    $server_stop = 10
  end
end

def server_stop
  $m.synchronize do
    if !server_running
      $server_stop = 0
      return
    end
    pid = $server_pid
    $server_pid  = nil
    $server_stop = 0
    Process.kill(pid)
  end
end

def server_running
  return !$server_pid.nil?
end

trap(:SIGCHLD) do |sig|
  $server_pid = nil
end

Thread.new do
  loop do
    sleep 1
    next if server_running
    next if $server_stop <= 0
    $server_stop -= 1
    server_start if $server_stop <= 0
  end
end

server_start
webrick.start
