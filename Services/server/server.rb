require 'webrick'
require 'thread'

$binary_name      = ENV['BINARY_NAME']      || "Server.x86_64"
$server_args      = ENV['SERVER_ARGS']      || nil
$player_log_path  = ENV['PLAYER_LOG_PATH']  || "DefaultCompany/NEWG/Player.log"
$unity3d_log_path = ENV['UNITY3D_LOG_PATH'] || "#{ENV['HOME']}/.config/unity3d"
$server_cmd       = './Builds/' + $binary_name
$server_cmd      += ' ' + $server_args if !$server_args.nil?
$server_pid       = nil
$server_stop      = 0
$m                = Mutex.new

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
    Process.kill("KILL", pid)
  end
end

def server_running
  return !$server_pid.nil?
end

trap(:SIGCHLD) do |sig|
  $server_pid = nil
end

# log loop
Thread.new do
  loop do
    sleep 1
    begin
      f = open(File.join($unity3d_log_path, $player_log_path))
      f.sysseek(-32, IO::SEEK_END) rescue f.sysseek(0, IO::SEEK_SET)
      loop do
        begin
          print f.sysread(10)
        rescue => e
          raise e if !e.is_a?(EOFError)
        end
      end
    rescue => e
      p e
      next
    end
  end
end

# server restart loop
Thread.new do
  loop do
    sleep 1
    next if server_running || $server_stop <= 0
    $server_stop -= 1
    server_start if $server_stop <= 0
  end
end

server_start
webrick.start
