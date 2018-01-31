require 'erb'
require 'yaml'
require 'optparse'
require 'fileutils'

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

templates.each do |template_info|
  desc          = template_info["desc"]
  projects_path = template_info["projects_path"]
  generate_path = template_info["generate_path"]
  template      = template_info["template"]
  puts "projects '#{projects_path}' (#{desc}) ..."
  dirs = Dir["#{projects_path}/*"]
  dirs.select{|p| File.directory?(p)}.each do |p|
    output_path  = "#{p}/#{generate_path}"
    output_dir   = File.dirname(output_path)
    project_name = File.basename(p)
    puts "  generate '#{output_path}' ..."
    if params[:go]
      FileUtils.rm_rf(output_dir) if Dir.exists?(output_dir)
      FileUtils.mkdir_p(output_dir)
      File.write(output_path, generate(template, models, project_name))
    else
      puts generate(template, models, project_name)
    end
  end
end
