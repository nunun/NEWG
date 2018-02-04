require 'erb'
require 'yaml'
require 'optparse'
require 'fileutils'
require 'pathname'

params = {:go => false}
opt = OptionParser.new
opt.on('-c') {|v| params[:go] = v }
opt.parse!(ARGV)

spec = YAML.load_file('./spec.yml')
models    = spec["models"]
templates = spec["templates"]

def generate(template, models, project_name)
  return ERB.new(template, nil, "-").result(binding);
end

removed_dirs = {}
templates.each do |template_info|
  projects_path = template_info["projects_path"]
  generate_path = template_info["generate_path"]
  template      = template_info["template"]
  raise "projects path empty!" if projects_path.empty?
  raise "generate path empty!" if generate_path.empty?
  puts "projects '#{projects_path}' ..."
  dirs = Dir["#{projects_path}/*"]
  dirs.select{|p| File.directory?(p)}.each do |p|
    output_path  = "#{p}/#{generate_path}"
    output_dir   = File.dirname(output_path)
    remove_paths = Pathname.new(generate_path).each_filename.to_a
    remove_dir   = "#{p}/#{remove_paths[0]}"
    raise "remove path empty?" if remove_paths[0].empty?
    project_name = File.basename(p)
    puts "  generate '#{output_path}' ..."
    if params[:go]
      if !removed_dirs.key?(remove_dir)
        removed_dirs[remove_dir] = true; # mark once removed.
        FileUtils.rm_rf(remove_dir) if Dir.exists?(remove_dir)
      end
      FileUtils.mkdir_p(output_dir)
      File.write(output_path, generate(template, models, project_name))
    else
      puts generate(template, models, project_name)
    end
  end
end
