require 'erb'
require 'yaml'
require 'json'
require 'optparse'
require 'fileutils'
require 'pathname'

params = {:go => false}
opt = OptionParser.new
opt.on('-c') {|v| params[:go] = v }
opt.parse!(ARGV)
params[:filter] = ARGV

# class extensions
class String
  # to pascal case (MySymbol)
  def to_pascal()
    self.to_words.map{|w| w[0] = w[0].upcase; w}.join
  end

  # to camel case (mySymbol)
  def to_camel()
    words = self.to_words.map{|w| w[0] = w[0].upcase; w}
    words[0] = words[0].downcase
    words.join
  end

  # to snake case (my_symbol)
  def to_snake()
    self.to_words.join("_").downcase
  end

  # to constant case (MY_SYMBOL)
  def to_const()
    self.to_words.join("_").upcase
  end

  # split symbol to words
  def to_words()
    process_special_words(self
      .gsub(/([A-Z]+)([A-Z][a-z])/, '\1_\2')
      .gsub(/([a-z\d])([A-Z])/, '\1_\2')
      .tr("-", "_")
      .split("_"))
  end

  # process special words
  # merge special words to one,
  # or split special word to words.
  def process_special_words(words)
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

# generate
def generate(spec_name, entries, templates, params, removed_dirs)
  templates.each do |template|
    generate_name = template["generate"]
    in_path       = template["in"]
    output_path   = template["output"]
    template_text = template["template"]
    eval_code     = template["eval"]

    raise "template property 'generate' is empty" if generate_name.to_s.empty?
    next if generate_name != spec_name

    if !eval_code.to_s.empty?  # eval mode
      raise "eval: property 'generate' must be 'before'." if generate_name != 'before'
      raise "eval: property 'in' must be empty."          if !in_path.to_s.empty?
      raise "eval: property 'output' must be empty."      if !output_path.to_s.empty?
      raise "eval: property 'template' must be empty."    if !template_text.to_s.empty?

      eval eval_code

    elsif !template_text.to_s.empty?  # output mode
      raise "template: property 'in' is empty."        if in_path.to_s.empty?
      raise "template: property 'output' is empty."    if output_path.to_s.empty?
      raise "template: property 'eval' must be empty." if !eval_code.to_s.empty?

      in_dirs = Dir["#{in_path}"]
      in_dirs.each do |in_dir|
        protocols_name = File.basename(in_dir)
        raise "'in' directory must named 'protocols' in case insensitive." if protocols_name.casecmp("PROTOCOLS") != 0

        output_file = File.join(in_dir, ERB.new(output_path, nil, "-").result(binding));
        output_dir  = File.dirname(output_file)

        if !params[:filter].nil?
          match_all = true
          params[:filter].each {|f| match_all &= output_file.include?(f)}
          next if !match_all
        end

        puts "generate '#{output_file}' ..."
        generated_text = ERB.new(template_text, nil, "-").result(binding)

        if params[:go]
          if !removed_dirs.include?(in_dir)
            removed_dirs.push(in_dir)
            FileUtils.rm_rf(in_dir)
            FileUtils.mkdir_p(in_dir)
          end
          FileUtils.mkdir_p(output_dir)
          File.write(output_file, generated_text)
        else
          puts generated_text
        end
      end

    else # error!
      raise "template property 'template' or 'eval' is empty."
    end
  end
end

# output
specs_yml = YAML.load_file('./specs.yml')
specs     = specs_yml["specs"]["services"]["protocols"]
templates = specs_yml["templates"]
removed_dirs = []
generate("before", {}, templates, params, removed_dirs)
specs.each do |spec_name,entries|
  generate(spec_name, entries, templates, params, removed_dirs)
end
generate("after", {}, templates, params, removed_dirs)
