require 'erb'
require 'yaml'
require 'optparse'
require 'fileutils'
require 'pathname'

params = {:go => false}
opt = OptionParser.new
opt.on('-c') {|v| params[:go] = v }
opt.parse!(ARGV)

specs_yml = YAML.load_file('./specs.yml')
specs     = specs_yml["specs"]
templates = specs_yml["templates"]

class String
  def to_pascal()
    self.to_words.map{|w| w[0] = w[0].upcase; w}.join
  end
  def to_camel()
    words = self.to_words.map{|w| w[0] = w[0].upcase; w}
    words[0] = words[0].downcase
    words.join
  end
  def to_snake()
    self.to_words.join("_").downcase
  end
  def to_const()
    self.to_words.join("_").upcase
  end
  def to_words()
    fix_special_words(self
      .gsub(/([A-Z]+)([A-Z][a-z])/, '\1_\2')
      .gsub(/([a-z\d])([A-Z])/, '\1_\2')
      .tr("-", "_")
      .split("_"))
  end
  def fix_special_words(words)
    result = []
    while !words.empty?
      if words.first.casecmp("WEBAPI") == 0 # WEBAPI => Web, API
        words.shift; result.push("Web"); result.push("API")
      else
        result.push(words.shift);
        if !words.empty?
          if result.last.casecmp("WEB") == 0 && words.first.casecmp("API") == 0 # WEB, API => WEBAPI
            result[result.length - 1] = result.last + words.shift
          end
        end
      end
    end
    result
  end
end

specs.each do |spec_name,spec|
  spec.each do |spec_generate_name,spec_definitions|
    templates.each do |template|
      generate_name = template["generate"]
      in_path       = template["in"]
      output_path   = template["output"]

      raise "template property 'generate' is empty" if generate_name.to_s.empty?
      raise "template property 'in' is empty"       if in_path.to_s.empty?
      raise "template property 'output' is empty"   if output_path.to_s.empty?

      break if generate_name != spec_generate_name

      output_dirs = Dir["#{in_path}/*"]
      output_dirs.map{|d| File.basename(d)}.each do |output_name|
        output_path = ERB.new(output_path, nil, "-").result(binding);
        p output_path
      end
    end
  end
end

#def generate(template, models, project_name)
#  return ERB.new(template, nil, "-").result(binding);
#end
#removed_dirs = {}
#templates.each do |template_info|
#  projects_path = template_info["projects_path"]
#  generate_path = template_info["generate_path"]
#  template      = template_info["template"]
#  raise "projects path empty!" if projects_path.empty?
#  raise "generate path empty!" if generate_path.empty?
#  puts "projects '#{projects_path}' ..."
#  dirs = Dir["#{projects_path}/*"]
#  dirs.select{|p| File.directory?(p)}.each do |p|
#    output_path  = "#{p}/#{generate_path}"
#    output_dir   = File.dirname(output_path)
#    remove_paths = Pathname.new(generate_path).each_filename.to_a
#    remove_dir   = "#{p}/#{remove_paths[0]}"
#    raise "remove path empty?" if remove_paths[0].empty?
#    project_name = File.basename(p)
#    puts "  generate '#{output_path}' ..."
#    if params[:go]
#      if !removed_dirs.key?(remove_dir)
#        removed_dirs[remove_dir] = true; # mark once removed.
#        FileUtils.rm_rf(remove_dir) if Dir.exists?(remove_dir)
#      end
#      FileUtils.mkdir_p(output_dir)
#      File.write(output_path, generate(template, models, project_name))
#    else
#      puts generate(template, models, project_name)
#    end
#  end
#end
