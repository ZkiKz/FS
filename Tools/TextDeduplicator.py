#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
智能文本去重工具 - 专为Unity TextMeshPro自定义字体字符集生成设计
功能：
1. 智能字符分类（汉字、字母、数字、标点、符号、空格等）
2. 多种去重模式和选项
3. 按字符类型分组输出
4. 详细的统计报告
5. 支持批量文件处理
6. 支持Unicode范围分析
7. 自动清理不可见字符

使用方法：
python TextDeduplicator.py [选项]
或直接运行交互式模式
"""

import os
import sys
import json
import re
from collections import Counter, OrderedDict
from typing import Set, Dict, List, Tuple

class TextDeduplicator:
    """智能文本去重器"""
    
    def __init__(self):
        # 字符分类集合
        self.chinese_chars = set()  # 汉字
        self.english_upper = set()  # 大写英文字母
        self.english_lower = set()  # 小写英文字母
        self.numbers = set()        # 数字
        self.chinese_punctuation = set()  # 中文标点
        self.english_punctuation = set()  # 英文标点
        self.special_symbols = set()  # 特殊符号
        self.spaces = set()         # 空格类字符
        self.other_chars = set()    # 其他字符
        
        # 统计信息
        self.stats = {
            'total_chars': 0,
            'unique_chars': 0,
            'chinese_count': 0,
            'english_upper_count': 0,
            'english_lower_count': 0,
            'number_count': 0,
            'chinese_punct_count': 0,
            'english_punct_count': 0,
            'special_symbol_count': 0,
            'space_count': 0,
            'other_count': 0,
        }
    
    def classify_char(self, char: str) -> str:
        """
        智能分类单个字符
        返回字符类型标识
        """
        code_point = ord(char)
        
        # 控制字符（不可见）
        if code_point < 32 or (127 <= code_point < 160):
            return 'control'
        
        # 空格类
        if char in ' \t\n\r\f\v\u00a0\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u2028\u2029\u3000':
            return 'space'
        
        # 数字
        if '0' <= char <= '9':
            return 'number'
        
        # 英文字母
        if 'A' <= char <= 'Z':
            return 'english_upper'
        if 'a' <= char <= 'z':
            return 'english_lower'
        
        # 汉字（CJK统一汉字）
        if '\u4e00' <= char <= '\u9fff' or '\u3400' <= char <= '\u4dbf' or '\u20000' <= char <= '\u2a6df':
            return 'chinese'
        
        # 中文标点
        if char in '，。、；：！？【】「」『』《》（）〔〕｛｝〟﹁﹂﹃﹄＂＇´｀¨ˊˋ˙–—…～‧':
            return 'chinese_punct'
        
        # 英文标点
        if char in ',.;:!?[](){}\'\"`~^&*()_+-=|\\<>/@#$%^&*':
            return 'english_punct'
        
        # Emoji和特殊符号
        if '\U0001f300' <= char <= '\U0001f9ff' or '\u2600' <= char <= '\u27ff' or '\u2300' <= char <= '\u23ff':
            return 'special_symbol'
        
        # 其他符号
        if '\u2000' <= char <= '\u206f' or '\u2200' <= char <= '\u22ff' or '\u2500' <= char <= '\u257f':
            return 'special_symbol'
        
        return 'other'
    
    def process_text(self, text: str, options: Dict = None) -> Dict:
        """
        处理文本并进行分类去重
        options: 处理选项
        """
        if options is None:
            options = {}
        
        # 重置统计
        self.chinese_chars.clear()
        self.english_upper.clear()
        self.english_lower.clear()
        self.numbers.clear()
        self.chinese_punctuation.clear()
        self.english_punctuation.clear()
        self.special_symbols.clear()
        self.spaces.clear()
        self.other_chars.clear()
        
        for key in self.stats:
            self.stats[key] = 0
        
        # 统计总字符数
        self.stats['total_chars'] = len(text)
        
        # 逐个字符分类
        for char in text:
            char_type = self.classify_char(char)
            
            # 根据选项过滤
            if options.get('remove_spaces') and char_type == 'space':
                continue
            if options.get('remove_control') and char_type == 'control':
                continue
            if options.get('remove_emoji') and char_type == 'special_symbol':
                continue
            
            # 分类存储
            if char_type == 'chinese':
                self.chinese_chars.add(char)
            elif char_type == 'english_upper':
                self.english_upper.add(char)
            elif char_type == 'english_lower':
                self.english_lower.add(char)
            elif char_type == 'number':
                self.numbers.add(char)
            elif char_type == 'chinese_punct':
                self.chinese_punctuation.add(char)
            elif char_type == 'english_punct':
                self.english_punctuation.add(char)
            elif char_type == 'special_symbol':
                self.special_symbols.add(char)
            elif char_type == 'space':
                self.spaces.add(char)
            elif char_type == 'other':
                self.other_chars.add(char)
        
        # 计算统计信息
        self.stats['chinese_count'] = len(self.chinese_chars)
        self.stats['english_upper_count'] = len(self.english_upper)
        self.stats['english_lower_count'] = len(self.english_lower)
        self.stats['number_count'] = len(self.numbers)
        self.stats['chinese_punct_count'] = len(self.chinese_punctuation)
        self.stats['english_punct_count'] = len(self.english_punctuation)
        self.stats['special_symbol_count'] = len(self.special_symbols)
        self.stats['space_count'] = len(self.spaces)
        self.stats['other_count'] = len(self.other_chars)
        self.stats['unique_chars'] = sum([
            self.stats['chinese_count'],
            self.stats['english_upper_count'],
            self.stats['english_lower_count'],
            self.stats['number_count'],
            self.stats['chinese_punct_count'],
            self.stats['english_punct_count'],
            self.stats['special_symbol_count'],
            self.stats['space_count'],
            self.stats['other_count']
        ])
        
        return self.get_results(options)
    
    def get_results(self, options: Dict = None) -> Dict:
        """获取处理结果"""
        if options is None:
            options = {}
        
        result = {
            'chinese': sorted(self.chinese_chars),
            'english_upper': sorted(self.english_upper),
            'english_lower': sorted(self.english_lower),
            'numbers': sorted(self.numbers),
            'chinese_punctuation': sorted(self.chinese_punctuation),
            'english_punctuation': sorted(self.english_punctuation),
            'special_symbols': sorted(self.special_symbols),
            'spaces': sorted(self.spaces),
            'others': sorted(self.other_chars),
            'stats': self.stats.copy()
        }
        
        # 如果只需要特定类型
        if options.get('only_chinese'):
            result = {k: v for k, v in result.items() if k == 'chinese'}
        elif options.get('only_letters'):
            result = {k: v for k, v in result.items() if k in ['english_upper', 'english_lower']}
        elif options.get('only_numbers'):
            result = {k: v for k, v in result.items() if k == 'numbers'}
        
        return result
    
    def format_output(self, result: Dict, format_type: str = 'grouped') -> str:
        """
        格式化输出结果
        format_type: 'grouped' 分组输出, 'flat' 扁平输出, 'json' JSON格式
        """
        if format_type == 'json':
            return json.dumps(result, ensure_ascii=False, indent=2)
        
        elif format_type == 'flat':
            # 扁平输出：所有字符连在一起
            all_chars = []
            for key in ['chinese', 'english_upper', 'english_lower', 'numbers', 
                       'chinese_punctuation', 'english_punctuation', 'special_symbols', 
                       'spaces', 'others']:
                if key in result:
                    all_chars.extend(result[key])
            return ''.join(all_chars)
        
        else:  # grouped - 分组输出
            lines = []
            group_names = {
                'chinese': '【汉字】',
                'english_upper': '【大写英文字母】',
                'english_lower': '【小写英文字母】',
                'numbers': '【数字】',
                'chinese_punctuation': '【中文标点】',
                'english_punctuation': '【英文标点】',
                'special_symbols': '【特殊符号/Emoji】',
                'spaces': '【空格类】',
                'others': '【其他字符】'
            }
            
            for key, name in group_names.items():
                if key in result and result[key]:
                    chars = result[key]
                    chars_per_line = 50
                    lines.append(f"\n{name}")
                    lines.append("=" * 60)
                    
                    for i in range(0, len(chars), chars_per_line):
                        chunk = chars[i:i + chars_per_line]
                        lines.append(''.join(chunk))
            
            return '\n'.join(lines)
    
    def generate_stats_report(self, result: Dict) -> str:
        """生成详细的统计报告"""
        stats = result['stats']
        lines = [
            "\n" + "=" * 60,
            "📊 字符统计报告",
            "=" * 60,
            f"总字符数（含重复）: {stats['total_chars']}",
            f"去重后字符数: {stats['unique_chars']}",
            f"重复率: {((stats['total_chars'] - stats['unique_chars']) / stats['total_chars'] * 100) if stats['total_chars'] > 0 else 0:.2f}%",
            "",
            "字符分类统计:",
            f"  汉字: {stats['chinese_count']} 个",
            f"  大写英文字母: {stats['english_upper_count']} 个",
            f"  小写英文字母: {stats['english_lower_count']} 个",
            f"  数字: {stats['number_count']} 个",
            f"  中文标点: {stats['chinese_punct_count']} 个",
            f"  英文标点: {stats['english_punct_count']} 个",
            f"  特殊符号/Emoji: {stats['special_symbol_count']} 个",
            f"  空格类: {stats['space_count']} 个",
            f"  其他字符: {stats['other_count']} 个",
            "=" * 60
        ]
        
        # Unicode范围分析
        if result.get('chinese'):
            chars = result['chinese']
            code_points = [ord(c) for c in chars]
            lines.append(f"\n汉字Unicode范围: U+{min(code_points):04X} ~ U+{max(code_points):04X}")
        
        return '\n'.join(lines)
    
    def process_file(self, filepath: str, options: Dict = None) -> Dict:
        """处理单个文件"""
        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                text = f.read()
            return self.process_text(text, options)
        except Exception as e:
            print(f"❌ 读取文件失败 {filepath}: {e}")
            return None
    
    def process_multiple_files(self, filepaths: List[str], options: Dict = None) -> Dict:
        """批量处理多个文件"""
        combined_text = ""
        
        for filepath in filepaths:
            try:
                with open(filepath, 'r', encoding='utf-8') as f:
                    combined_text += f.read()
            except Exception as e:
                print(f"⚠️ 跳过文件 {filepath}: {e}")
        
        return self.process_text(combined_text, options)
    
    def process_directory(self, dirpath: str, extensions: List[str] = None, options: Dict = None) -> Dict:
        """处理目录下的所有文件"""
        if extensions is None:
            extensions = ['.txt', '.cs', '.json', '.xml', '.lua']
        
        filepaths = []
        for root, dirs, files in os.walk(dirpath):
            for file in files:
                if any(file.endswith(ext) for ext in extensions):
                    filepaths.append(os.path.join(root, file))
        
        print(f"📁 找到 {len(filepaths)} 个文件")
        return self.process_multiple_files(filepaths, options)


def print_usage():
    """打印使用说明"""
    print("""
╔═══════════════════════════════════════════════════════════════╗
║           Unity TMP 智能文本去重工具 v1.0                      ║
║           专为自定义字体字符集生成设计                         ║
╚═══════════════════════════════════════════════════════════════╝

使用方法:
  python TextDeduplicator.py [选项]

选项:
  -i, --input FILE         输入文件路径
  -d, --dir DIR            输入目录路径（批量处理）
  -o, --output FILE        输出文件路径（默认: output_charset.txt）
  -m, --mode MODE          去重模式: 
                           all(完全去重) | chinese(仅汉字) | 
                           letters(仅字母) | numbers(仅数字) |
                           nopunct(无标点) | nospace(无空格)
  -f, --format FORMAT      输出格式: 
                           grouped(分组，默认) | flat(扁平) | json
  --remove-spaces          移除所有空格
  --remove-control         移除控制字符
  --remove-emoji           移除Emoji和特殊符号
  --stats-only             仅显示统计信息
  --inplace                直接修改原文件（将原文件内容替换为去重结果）
  -h, --help               显示此帮助信息

示例:
  # 处理文件并输出分组格式的字符集（默认）
  python TextDeduplicator.py -i input.txt --inplace
  
  # 直接修改原文件，将内容替换为分类格式的字符集
  python TextDeduplicator.py -i input.txt --inplace
  
  # 只提取汉字并直接修改原文件
  python TextDeduplicator.py -i input.txt -m chinese --inplace
  
  # 扁平格式输出（所有字符在一行）
  python TextDeduplicator.py -i input.txt -f flat --inplace
  
  # 批量处理目录
  python TextDeduplicator.py -d ./scripts -m chinese -o chinese_chars.txt
  
  # 去除空格和控制字符
  python TextDeduplicator.py -i ui_texts.txt --remove-spaces --remove-control --inplace
  
  # JSON格式输出
  python TextDeduplicator.py -i game_texts.txt -f json -o charset.json

""")


def main():
    """主函数"""
    import argparse
    
    parser = argparse.ArgumentParser(description='Unity TMP 智能文本去重工具', add_help=False)
    parser.add_argument('-i', '--input', help='输入文件路径')
    parser.add_argument('-d', '--dir', help='输入目录路径')
    parser.add_argument('-o', '--output', default='output_charset.txt', help='输出文件路径')
    parser.add_argument('-m', '--mode', default='all', 
                       choices=['all', 'chinese', 'letters', 'numbers', 'nopunct', 'nospace'])
    parser.add_argument('-f', '--format', default='grouped', 
                       choices=['grouped', 'flat', 'json'])
    parser.add_argument('--remove-spaces', action='store_true')
    parser.add_argument('--remove-control', action='store_true')
    parser.add_argument('--remove-emoji', action='store_true')
    parser.add_argument('--stats-only', action='store_true')
    parser.add_argument('--inplace', action='store_true', help='直接修改原文件')
    parser.add_argument('-h', '--help', action='store_true')
    
    args = parser.parse_args()
    
    if args.help:
        print_usage()
        return
    
    deduplicator = TextDeduplicator()
    
    # 构建选项
    options = {
        'remove_spaces': args.remove_spaces,
        'remove_control': args.remove_control,
        'remove_emoji': args.remove_emoji,
    }
    
    if args.mode == 'chinese':
        options['only_chinese'] = True
    elif args.mode == 'letters':
        options['only_letters'] = True
    elif args.mode == 'numbers':
        options['only_numbers'] = True
    elif args.mode == 'nopunct':
        options['remove_punctuation'] = True
    elif args.mode == 'nospace':
        options['remove_spaces'] = True
    
    # 处理输入
    result = None
    
    if args.input:
        print(f"📄 处理文件: {args.input}")
        result = deduplicator.process_file(args.input, options)
        
        if result is not None:
            # 生成去重后的字符集（只生成纯字符集，不含统计报告）
            if args.format == 'json':
                output_text = deduplicator.format_output(result, 'json')
            elif args.format == 'flat':
                output_text = deduplicator.format_output(result, 'flat')
            else:
                output_text = deduplicator.format_output(result, 'grouped')
            
            # 如果指定了inplace，直接修改原文件，将内容完全替换为去重结果
            if args.inplace:
                with open(args.input, 'w', encoding='utf-8') as f:
                    f.write(output_text)
                print(f"✅ 已直接修改原文件: {args.input}")
                print(f"   文件内容已替换为去重后的字符集")
            else:
                # 否则输出到新文件
                with open(args.output, 'w', encoding='utf-8') as f:
                    f.write(output_text)
                print(f"✅ 结果已保存到: {args.output}")
            
            # 显示统计信息（只在控制台显示，不写入文件）
            print("\n" + "=" * 60)
            print("📊 字符统计报告（仅供参考，未写入文件）")
            print("=" * 60)
            stats = result['stats']
            print(f"总字符数（含重复）: {stats['total_chars']}")
            print(f"去重后字符数: {stats['unique_chars']}")
            print(f"重复率: {((stats['total_chars'] - stats['unique_chars']) / stats['total_chars'] * 100) if stats['total_chars'] > 0 else 0:.2f}%")
            print(f"\n字符分类统计:")
            print(f"  汉字: {stats['chinese_count']} 个")
            print(f"  大写英文字母: {stats['english_upper_count']} 个")
            print(f"  小写英文字母: {stats['english_lower_count']} 个")
            print(f"  数字: {stats['number_count']} 个")
            print(f"  中文标点: {stats['chinese_punct_count']} 个")
            print(f"  英文标点: {stats['english_punct_count']} 个")
            print(f"  特殊符号/Emoji: {stats['special_symbol_count']} 个")
            print(f"  空格类: {stats['space_count']} 个")
            print(f"  其他字符: {stats['other_count']} 个")
            print("=" * 60)
            print(f"\n💡 文件中的内容已是纯净的去重字符集，可直接用于Unity TMP自定义字体")
    
    elif args.dir:
        print(f"📁 处理目录: {args.dir}")
        result = deduplicator.process_directory(args.dir, options=options)
        
        if result is not None:
            # 生成去重后的字符集
            if args.format == 'json':
                output_text = deduplicator.format_output(result, 'json')
            elif args.format == 'flat':
                output_text = deduplicator.format_output(result, 'flat')
            else:
                output_text = deduplicator.format_output(result, 'grouped')
            
            # 输出到文件
            with open(args.output, 'w', encoding='utf-8') as f:
                f.write(output_text)
            print(f"✅ 结果已保存到: {args.output}")
            
            # 显示统计信息
            print("\n" + "=" * 60)
            print("📊 字符统计报告（仅供参考，未写入文件）")
            print("=" * 60)
            stats = result['stats']
            print(f"总字符数（含重复）: {stats['total_chars']}")
            print(f"去重后字符数: {stats['unique_chars']}")
            print(f"重复率: {((stats['total_chars'] - stats['unique_chars']) / stats['total_chars'] * 100) if stats['total_chars'] > 0 else 0:.2f}%")
            print(f"\n字符分类统计:")
            print(f"  汉字: {stats['chinese_count']} 个")
            print(f"  大写英文字母: {stats['english_upper_count']} 个")
            print(f"  小写英文字母: {stats['english_lower_count']} 个")
            print(f"  数字: {stats['number_count']} 个")
            print(f"  中文标点: {stats['chinese_punct_count']} 个")
            print(f"  英文标点: {stats['english_punct_count']} 个")
            print(f"  特殊符号/Emoji: {stats['special_symbol_count']} 个")
            print(f"  空格类: {stats['space_count']} 个")
            print(f"  其他字符: {stats['other_count']} 个")
            print("=" * 60)
            print(f"\n💡 文件中的内容已是纯净的去重字符集，可直接用于Unity TMP自定义字体")
    
    else:
        # 交互式模式
        print("🎮 进入交互式模式")
        print("请输入文本（输入空行结束）:")
        lines = []
        while True:
            line = input()
            if not line:
                break
            lines.append(line)
        
        text = '\n'.join(lines)
        result = deduplicator.process_text(text, options)
        
        if result is not None:
            # 生成去重后的字符集
            if args.format == 'json':
                output_text = deduplicator.format_output(result, 'json')
            elif args.format == 'flat':
                output_text = deduplicator.format_output(result, 'flat')
            else:
                output_text = deduplicator.format_output(result, 'grouped')
            
            # 输出到文件
            with open(args.output, 'w', encoding='utf-8') as f:
                f.write(output_text)
            print(f"✅ 结果已保存到: {args.output}")
            
            # 显示统计信息
            print("\n" + "=" * 60)
            print("📊 字符统计报告（仅供参考，未写入文件）")
            print("=" * 60)
            stats = result['stats']
            print(f"总字符数（含重复）: {stats['total_chars']}")
            print(f"去重后字符数: {stats['unique_chars']}")
            print(f"重复率: {((stats['total_chars'] - stats['unique_chars']) / stats['total_chars'] * 100) if stats['total_chars'] > 0 else 0:.2f}%")
            print(f"\n字符分类统计:")
            print(f"  汉字: {stats['chinese_count']} 个")
            print(f"  大写英文字母: {stats['english_upper_count']} 个")
            print(f"  小写英文字母: {stats['english_lower_count']} 个")
            print(f"  数字: {stats['number_count']} 个")
            print(f"  中文标点: {stats['chinese_punct_count']} 个")
            print(f"  英文标点: {stats['english_punct_count']} 个")
            print(f"  特殊符号/Emoji: {stats['special_symbol_count']} 个")
            print(f"  空格类: {stats['space_count']} 个")
            print(f"  其他字符: {stats['other_count']} 个")
            print("=" * 60)
            print(f"\n💡 文件中的内容已是纯净的去重字符集，可直接用于Unity TMP自定义字体")
    
    if result is None and args.input is None and args.dir is None:
        print("❌ 处理失败")

if __name__ == '__main__':
    main()
