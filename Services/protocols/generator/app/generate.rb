require 'erb'
require 'yaml'

spec = YAML.load_file('./spec.yml')

def generate(template_name, spec)
  models    = spec["models"]
  templates = spec["templates"]
  return ERB.new(templates[template_name], nil, "-").result(binding);
end

puts generate("nodejs", spec);
