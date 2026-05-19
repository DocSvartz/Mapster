import os
import glob
import re
from bs4 import BeautifulSoup

# --- SETTINGS ---
SITE_DIR = "_site"
TAGS_REFERENCE_FILE = "tags-reference.md"

# --- REGEX PATTERN FOR TAG VALIDATION ---
# Matches patterns like: v111.222.333 or v111.222.333+
# \d+ — one or more digits
PATTERN = re.compile(r'^v\d+\.\d+\.\d+\+?$')

def build_tag_index():
    """Builds an index of tags matching the pattern vX.X.X[+]."""
    tag_index = {}

    print(f"--- Searching for tags in folder: {os.path.abspath(SITE_DIR)} ---")

    html_files = glob.glob(os.path.join(SITE_DIR, '**', '*.html'), recursive=True)

    if not html_files:
        print("\nERROR: No .html files found in the _site folder.")
        print("Make sure you run the script AFTER the 'docfx' command.")
        return {}

    print(f"Found {len(html_files)} HTML files. Processing...")
    print("-" * 70)

    for file_path in html_files:
        rel_path = os.path.relpath(file_path, SITE_DIR).replace(os.path.sep, '/')
        
        # Path and extension fix
        if rel_path.startswith('_site/'):
            rel_path = rel_path[6:]
        if rel_path.endswith('.html'):
            rel_path = rel_path[:-5] + '.md'
            
        final_link_path = f"~/{rel_path}"

        try:
            with open(file_path, 'r', encoding='utf-8-sig') as f:
                content = f.read()

            soup = BeautifulSoup(content, 'html.parser')

            for header in soup.find_all(['h1', 'h2', 'h3', 'h4', 'h5', 'h6']):
                full_text = header.get_text()
                anchor_id = header.get('id')
                
                if '[' in full_text and ']' in full_text and anchor_id:
                    link = f"[{full_text.strip()}]({final_link_path}#{anchor_id})"

                    # --- MAIN CHANGE: TAG FILTERING ---
                    for part in full_text.split('['):
                        if ']' in part:
                            # Extract text inside brackets and trim spaces
                            tag = part.split(']', 1)[0].strip()
                            
                            # Check if the tag matches our pattern
                            if tag and PATTERN.match(tag):
                                tag_index.setdefault(tag, []).append(link)

        except Exception as e:
            print(f"⚠️ Error processing file {rel_path}: {e}")
    
    return tag_index

def create_tags_reference_file(index_data):
    """Creates the tags-reference.md file with a table of contents by tags."""
    if not index_data:
        print("\n❌ Index is empty. No tags were found. File will not be created.")
        return

    print(f"\n--- Creating TOC file: {TAGS_REFERENCE_FILE} ---")
    
    markdown_content = "# Tag Links\n\n"
    markdown_content += "This section collects all places in the documentation marked with tags.\n\n"

    for tag in sorted(index_data.keys()):
        markdown_content += f"## Tag: `{tag}`\n\n"
        
        for link in sorted(index_data[tag]):
            markdown_content += f"- {link}\n"
            
        markdown_content += "\n" 

    try:
        with open(TAGS_REFERENCE_FILE, 'w', encoding='utf-8') as f:
            f.write(markdown_content)
        
        file_size_kb = os.path.getsize(TAGS_REFERENCE_FILE) / 1024
        print(f"🎉 SUCCESS: File '{TAGS_REFERENCE_FILE}' created successfully!")
        print(f"   File size: {file_size_kb:.2f} KB")
        print(f"   Number of tags in TOC: {len(index_data)}")
        
    except Exception as e:
        print(f"\n❌ ERROR writing file: {e}")


if __name__ == "__main__":
    print("Starting tag analysis and TOC generation...")
    
    index_data = build_tag_index()
    
    if index_data:
        create_tags_reference_file(index_data)